using System;
using System.Dynamic;

namespace TouchBubbles.Shared.Models
{
    public class HomeAssistantConfiguration
    {
        public string HomeAssistantApi { get; set; } = "http://supervisor/core";

        public string SupervisorToken { get; set; } = "not configured";

        public string DataDirectory { get; set; } = "/data";
    }
}
