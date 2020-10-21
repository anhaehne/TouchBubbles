using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TouchBubbles.Client.Services;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Client.Models.Bubbles
{
    public class LightBubble : EntityBubble
    {
        private readonly IEntityService _entityService;
        private int _brightness;

        public LightBubble(Entity entity, IEntityService entityService)
         : base(entity)
        {
            if (entity.Type != "light")
                throw new ArgumentException("Entity has to be a light entity.");

            SupportsSlidingValue = true;
            _entityService = entityService;
        }

        protected override void OnEntityChanged()
        {
            var attr = Entity.Attributes.ToObject<LightAttributes>();
            _brightness = attr.Brightness;
            SlidingValue = _brightness / 255f;
        }

        public override async Task OnClickAsync()
        {
            Entity.State = Entity.State == "on" ? "off" : "on";
            await _entityService.CallServiceAsync("light", "toggle", Entity.Id);
        }

        protected override async void OnSlidingValueChanged(float newValue)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (newValue < 0 || _entityService == null)
                return;

            var newBrightness = (int)Math.Round(newValue * 255);

            if (newBrightness == _brightness)
                return;

            _brightness = newBrightness;

            await _entityService.CallServiceAsync(
                "light",
                "turn_on",
                new { entity_id = Entity.Id, brightness = _brightness });
        }

        public class LightAttributes
        {
            [JsonPropertyName("brightness")]
            public int Brightness { get; set; }
        }
    }
}