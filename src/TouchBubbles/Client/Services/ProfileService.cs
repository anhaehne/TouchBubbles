using Blazored.LocalStorage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using TouchBubbles.Shared;
using TouchBubbles.Shared.Models;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Client.Services
{
    public interface IProfileService
    {
        BehaviorSubject<Profile> ActiveProfile { get; }

        IObservable<IReadOnlyCollection<Profile>> Profiles { get; }

        Task InitializeAsync();

        Task AddProfileAsync(string profileName);

        void SetActiveProfile(Profile profile);

        Task RemoveProfileAsync(Profile profile);

        void ProfileChanged(object sender, Profile e);
    }

    public class ProfileService : IProfileService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IEntityService _entityService;
        private readonly ILocalStorageService _localStorageService;
        private RangeObservableCollection<Profile> _profiles = new RangeObservableCollection<Profile>();
        private BehaviorSubject<Profile> _activeProfile = new BehaviorSubject<Profile>(Profile.Empty);
        private bool _isInitialized;

        public ProfileService(IHttpClientFactory httpClientFactory, IEntityService entityService, ILocalStorageService localStorageService)
        {
            _httpClientFactory = httpClientFactory;
            _entityService = entityService;
            _localStorageService = localStorageService;

            _activeProfile.SubscribeAsync(p => SetActiveProfileIdAsync(p.Id));
        }

        public BehaviorSubject<Profile> ActiveProfile => _activeProfile;

        public IObservable<IReadOnlyCollection<Profile>> Profiles => _profiles.ToCollectionObservable();
        
        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;
            
            _profiles.AddRange(await GetProfilesAsync());

            var activeProfileId = await GetActiveProfileIdAsync();

            _activeProfile.OnNext(_profiles.FirstOrDefault(x => x.Id == activeProfileId) ?? _profiles.First());

            _isInitialized = true;
        }

        public async Task AddProfileAsync(string profileName)
        {
            var profile = new Profile(profileName, _entityService.Entities);
            _profiles.Add(profile);

            await UpdateProfile(profile);
        }

        public void SetActiveProfile(Profile profile)
        {
            _activeProfile.OnNext(profile);
        }

        public async Task RemoveProfileAsync(Profile profile)
        {
            if (!_profiles.Contains(profile))
                throw new ArgumentException("Unknown profile.");

            if (_profiles.Count == 0)
                throw new InvalidOperationException("Can't delete the last profile.");

            if (profile == await ActiveProfile.LastAsync())
                _activeProfile.OnNext(_profiles.Except(new[] { profile }).First());

            _profiles.Remove(profile);

            var httpClient = _httpClientFactory.CreateClient(EndPoints.BackEnd);
            using var response = await httpClient.DeleteAsync($"profile/{profile.Id}");
        }

        public async void ProfileChanged(object? sender, Profile e)
        {
            await UpdateProfile(e);
        }

        private async Task<List<Profile>> GetProfilesAsync()
        {
            var httpClient = _httpClientFactory.CreateClient(EndPoints.BackEnd);
            var profileDtos = await httpClient.GetFromJsonAsync<ProfileDto[]>("profile") ?? Array.Empty<ProfileDto>();

            return profileDtos.Select(p => new Profile(p.Name, _entityService.Entities, p.Id, p.EntityIds)).ToList();
        }

        private const string ACTIVE_PROFILE_KEY = "ActiveProfile";

        private async Task<Guid> GetActiveProfileIdAsync()
        {
            if (!await _localStorageService.ContainKeyAsync(ACTIVE_PROFILE_KEY))
                return Guid.Empty;

            return await _localStorageService.GetItemAsync<Guid>(ACTIVE_PROFILE_KEY);
        }

        private async Task SetActiveProfileIdAsync(Guid id)
        {
            await _localStorageService.SetItemAsync(ACTIVE_PROFILE_KEY, id);
        }

        private async Task UpdateProfile(Profile profile)
        {
            var httpClient = _httpClientFactory.CreateClient(EndPoints.BackEnd);
            using var response = await httpClient.PostAsJsonAsync("profile", Profile.ToDto(profile));
        }
    }
}