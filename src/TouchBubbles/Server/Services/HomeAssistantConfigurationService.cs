using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TouchBubbles.Shared;
using TouchBubbles.Shared.Models;
using TouchBubbles.Shared.Models.HomeAssistant;
using TouchBubbles.Shared.Services;

namespace TouchBubbles.Server.Services
{
    public class HomeAssistantConfigurationService : IHomeAssistantConfigurationService
    {
        private readonly IOptions<HomeAssistantConfiguration> _haConfig;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeAssistantConfigurationService(
            IOptions<HomeAssistantConfiguration> haConfig,
            IHttpClientFactory httpClientFactory)
        {
            _haConfig = haConfig;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<HomeAssistantConfiguration> GetConfigurationAsync()
        {
            var haClient = _httpClientFactory.CreateClient(Constants.HomeAssistant);
            var discoveryInfo = await haClient.GetFromJsonAsync<DiscoveryInfo>("api/discovery_info");

            return new HomeAssistantConfiguration
                { HomeAssistantApi = discoveryInfo.BaseUrl, SupervisorToken = _haConfig.Value.SupervisorToken };
        }
    }
}