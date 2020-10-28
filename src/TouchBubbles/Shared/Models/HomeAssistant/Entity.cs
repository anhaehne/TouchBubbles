using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;

namespace TouchBubbles.Shared.Models.HomeAssistant
{
    public class Entity
    {
        private static Mapper _mapper =
            new Mapper(new MapperConfiguration(builder => builder.CreateMap<Entity, Entity>()));

        private string? _icon;

        [JsonPropertyName("entity_id")]
        public string Id { get; set; } = "unknown";

        [JsonIgnore]
        public string Type => Id?.Split(".").FirstOrDefault() ?? "unknown";

        [JsonIgnore]
        public string Name => GetName();

        [JsonIgnore]
        public string Icon
        {
            get => _icon ?? GetIconOrDefault();
            set => _icon = value;
        }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("attributes")]
        public JsonElement Attributes { get; set; }

        [JsonPropertyName("last_changed")]
        public DateTimeOffset LastChanged { get; set; }

        [JsonPropertyName("last_modified")]
        public DateTimeOffset LastModified { get; set; }

        public Dictionary<string, string>? Context { get; set; }

        public event Action? EntityChanged;

        public void UpdateWith(Entity entity)
        {
            _mapper.Map(entity, this);
            EntityChanged?.Invoke();
        }

        private string GetName()
        {
            if (Attributes.TryGetProperty("friendly_name", out var propValue) && !string.IsNullOrEmpty(propValue.GetString()))
                return propValue.GetString() ?? "unknown";

            var lastIdPart = Id?.Split(".").LastOrDefault();

            if (!string.IsNullOrEmpty(lastIdPart))
                return lastIdPart;

            return Id ?? "Unknown";
        }

        private string GetIconOrDefault()
        {
            const string DEFAULT = "mdi-puzzle";

            if (!Attributes.TryGetProperty("icon", out var propValue))
                return DEFAULT;

            var icon = propValue.GetString();

            if (string.IsNullOrEmpty(icon) || !icon.Contains("mdi:"))
                return DEFAULT;

            return icon.Replace(":", "-");
        }
    }
}