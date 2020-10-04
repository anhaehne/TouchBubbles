using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TouchBubbles.Shared.Models;

namespace TouchBubbles.Shared.Services
{
    public interface IHomeAssistantConfigurationService
    {
        Task<HomeAssistantConfiguration> GetConfigurationAsync();
    }
}
