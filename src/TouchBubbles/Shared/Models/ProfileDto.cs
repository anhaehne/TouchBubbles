using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Shared.Models
{
    public class ProfileDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("entity_ids")]
        public IReadOnlyCollection<string> EntityIds { get; set; }
    }
}