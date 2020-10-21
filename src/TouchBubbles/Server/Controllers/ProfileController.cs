using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TouchBubbles.Server.Services;
using TouchBubbles.Shared.Models;

namespace TouchBubbles.Server.Controllers
{
    [Route("profile")]
    public class ProfileController
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpPost]
        public async Task UpdateProfileAsync([FromBody]ProfileDto profile)
        {
            await _profileService.UpdateProfileAsync(profile);
        }

        [HttpGet]
        public async Task<IReadOnlyCollection<ProfileDto>> GetProfilesAsync()
        {
            return await _profileService.GetProfilesAsync();
        }

        [HttpDelete("{id}")]
        public async Task RemoveProfileAsync(Guid id)
        {
            await _profileService.RemoveProfileAsync(id);
        }
    }
}
