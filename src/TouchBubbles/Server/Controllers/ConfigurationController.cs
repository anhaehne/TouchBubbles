using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TouchBubbles.Shared.Services;

namespace TouchBubbles.Server.Controllers
{
    [Route("Config")]
    public class ConfigurationController : Controller
    {
        private readonly IHomeAssistantConfigurationService _homeAssistantConfigurationService;

        public ConfigurationController(IHomeAssistantConfigurationService homeAssistantConfigurationService)
        {
            _homeAssistantConfigurationService = homeAssistantConfigurationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            return Ok(await _homeAssistantConfigurationService.GetConfigurationAsync());
        }
    }
}
