using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TouchBubbles.Shared.Models.HomeAssistant
{
    public class Entity
    {
        [JsonPropertyName("entity_id")]
        public string Id { get; set; }

        [JsonIgnore]
        public string Type => Id?.Split(".").FirstOrDefault() ?? "Unknown";

        [JsonIgnore]
        public string Name => Attributes.ContainsKey("friendly_name") ? Attributes["friendly_name"].GetString() : Id?.Split(".").LastOrDefault();

        public string State { get; set; }

        public Dictionary<string, JsonElement> Attributes { get; set; }

        [JsonPropertyName("last_changed")]
        public DateTimeOffset LastChanged { get; set; }

        [JsonPropertyName("last_modified")]
        public DateTimeOffset LastModified { get; set; }

        public Dictionary<string, string> Context { get; set; }
    }
}
