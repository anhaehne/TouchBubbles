using System;
using System.Text.Json.Serialization;

namespace TouchBubbles.Shared.Models.HomeAssistant
{
    public class DiscoveryInfo
    {
        [JsonPropertyName("base_url")]
        public string BaseUrl { get; set; }

        [JsonPropertyName("location_name")]
        public string LocationName { get; set; }

        [JsonPropertyName("requires_api_password")]
        public bool RequiresApiPassword { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
}
