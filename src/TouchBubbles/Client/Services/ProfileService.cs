using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TouchBubbles.Shared;
using TouchBubbles.Shared.Models;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Client.Services
{
    public interface IProfileService
    {
        Profile ActiveProfile { get; set; }

        IReadOnlyCollection<Profile> Profiles { get; }

        event EventHandler<Profile>? ActiveProfileChanged;

        event EventHandler? ProfilesChanged;

        Task InitializeAsync();

        Task AddProfileAsync(Profile profile);

        Task RemoveProfileAsync(Profile profile);

        void ProfileChanged(object sender, Profile e);
    }

    public class ProfileService : IProfileService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IEntityService _entityService;
        private List<Profile> _profiles = new List<Profile>();
        private Profile _activeProfile = Profile.Empty;
        private bool _isInitialized;

        public ProfileService(IHttpClientFactory httpClientFactory, IEntityService entityService)
        {
            _httpClientFactory = httpClientFactory;
            _entityService = entityService;
        }

        public Profile ActiveProfile
        {
            get => _activeProfile;
            set
            {
                _activeProfile = value;
                ActiveProfileChanged?.Invoke(this, value);
            }
        }

        public IReadOnlyCollection<Profile> Profiles => _profiles;

        public event EventHandler<Profile>? ActiveProfileChanged;

        public event EventHandler? ProfilesChanged;

        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;
            
            _profiles = await GetProfilesAsync();
            _activeProfile = _profiles.First();

            _isInitialized = true;
        }

        public async Task AddProfileAsync(Profile profile)
        {
            _profiles.Add(profile);
            profile.Changed += ProfileChanged;
            ProfilesChanged?.Invoke(this, EventArgs.Empty);

            await UpdateProfile(profile);
        }

        public async Task RemoveProfileAsync(Profile profile)
        {
            if (!_profiles.Contains(profile))
                throw new ArgumentException("Unknown profile.");

            if (_profiles.Count == 0)
                throw new InvalidOperationException("Can't delete the last profile.");

            if (profile == ActiveProfile)
                ActiveProfile = _profiles.Except(new[] { profile }).First();

            profile.Changed -= ProfileChanged;
            _profiles.Remove(profile);
            ProfilesChanged?.Invoke(this, EventArgs.Empty);

            var httpClient = _httpClientFactory.CreateClient(EndPoints.BackEnd);
            using var response = await httpClient.DeleteAsync($"profile/{profile.Id}");
        }

        public async void ProfileChanged(object sender, Profile e)
        {
            if (e == ActiveProfile)
                ActiveProfileChanged?.Invoke(this, e);

            await UpdateProfile(e);
        }

        private async Task<List<Profile>> GetProfilesAsync()
        {
            var httpClient = _httpClientFactory.CreateClient(EndPoints.BackEnd);
            var profileDtos = await httpClient.GetFromJsonAsync<ProfileDto[]>("profile");

            var profiles = profileDtos.Select(p => new Profile(p.Name, p.Id, new ObservableCollection<Entity>(GetEntities(p.EntityIds)))).ToList();

            foreach (var profile in profiles)
            {
                profile.Changed += ProfileChanged;
            }

            return profiles;

            IEnumerable<Entity> GetEntities(IEnumerable<string> ids)
            {
                foreach (var id in ids)
                {
                    var entity = _entityService.Entities.SingleOrDefault(e => e.Id == id);

                    if (entity != null)
                        yield return entity;
                }
            }
        }

        private async Task UpdateProfile(Profile profile)
        {
            var httpClient = _httpClientFactory.CreateClient(EndPoints.BackEnd);
            using var response = await httpClient.PostAsJsonAsync("profile", Profile.ToDto(profile));
        }
    }
}