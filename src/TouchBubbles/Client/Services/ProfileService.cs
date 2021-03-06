﻿using Blazored.LocalStorage;
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
            _activeProfile.SubscribeAsync(p => SetActiveProfileIdAsync(p.Id));

            _isInitialized = true;
        }

        public async Task AddProfileAsync(string profileName)
        {
            var profile = new Profile(profileName, _entityService.Entities);
            profile.Changed += ProfileOnChanged;

            _profiles.Add(profile);

            await UpdateProfileAsync(profile);
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

            profile.Changed -= ProfileOnChanged;

            var httpClient = _httpClientFactory.CreateClient(EndPoints.BackEnd);
            using var response = await httpClient.DeleteAsync($"profile/{profile.Id}");
        }
        
        private async Task<List<Profile>> GetProfilesAsync()
        {
            var httpClient = _httpClientFactory.CreateClient(EndPoints.BackEnd);
            var profileDtos = await httpClient.GetFromJsonAsync<ProfileDto[]>("profile") ?? Array.Empty<ProfileDto>();

            var profiles = profileDtos.Select(p => new Profile(p.Name, _entityService.Entities, p.Id, p.EntityIds))
                .ToList();

            foreach (var profile in profiles)
                profile.Changed += ProfileOnChanged;

            return profiles;
        }

        private async void ProfileOnChanged(object? sender, Profile profile)
        {
            await UpdateProfileAsync(profile);
        }

        private async Task UpdateProfileAsync(Profile profile)
        {
            var httpClient = _httpClientFactory.CreateClient(EndPoints.BackEnd);
            using var response = await httpClient.PostAsJsonAsync("profile", Profile.ToDto(profile));
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
    }
}