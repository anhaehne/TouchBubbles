using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TouchBubbles.Shared.Models;

namespace TouchBubbles.Server.Services
{
    public interface IProfileService
    {
        Task UpdateProfileAsync(ProfileDto profile);
        Task<IReadOnlyCollection<ProfileDto>> GetProfilesAsync();
        Task RemoveProfileAsync(Guid id);
    }

    public class ProfileService : IProfileService
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly IOptions<HomeAssistantConfiguration> _config;

        public ProfileService(IOptions<HomeAssistantConfiguration> config)
        {
            _config = config;
        }

        public async Task UpdateProfileAsync(ProfileDto profile)
        {
            var profiles = (await GetProfilesAsync()).ToList();

            await _semaphore.WaitAsync();

            try
            {
                var existingProfile = profiles.SingleOrDefault(p => p.Id == profile.Id);

                if (existingProfile != null)
                    profiles.Remove(existingProfile);

                profiles.Add(profile);

                await WriteProfilesAsync(profiles);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IReadOnlyCollection<ProfileDto>> GetProfilesAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                var file = new FileInfo(GetConfigPath());

                if (!file.Exists)
                    return await CreateNewProfileConfigAsync();

                await using var fs = file.OpenRead();

                return await JsonSerializer.DeserializeAsync<ProfileDto[]>(fs);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task RemoveProfileAsync(Guid id)
        {
            await _semaphore.WaitAsync();

            try
            {
                var profiles = (await GetProfilesAsync()).ToList();

                // Can't delete last profile
                if (profiles.Count == 1)
                    return;

                var foundProfile = profiles.SingleOrDefault(p => p.Id == id);

                if (foundProfile == null)
                    return;

                profiles.Remove(foundProfile);

                await WriteProfilesAsync(profiles);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<IReadOnlyCollection<ProfileDto>> CreateNewProfileConfigAsync()
        {
            var profiles = new[]
                { new ProfileDto { Name = "Default", Id = Guid.NewGuid(), EntityIds = Array.Empty<string>()} };

            await WriteProfilesAsync(profiles);

            return profiles;
        }

        private async Task WriteProfilesAsync(IReadOnlyCollection<ProfileDto> profiles)
        {
            var file = new FileInfo(GetConfigPath());

            if (file.Exists)
                file.Delete();

            await using var fs = file.OpenWrite();
            await JsonSerializer.SerializeAsync(fs, profiles);
        }

        private string GetConfigPath()
        {
            Directory.CreateDirectory(_config.Value.DataDirectory);
            var path = Path.Join(_config.Value.DataDirectory, "profiles.json");

            return path;
        }
    }
}