using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TouchBubbles.Client.Services;
using TouchBubbles.Shared.Models;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Client.ViewModels
{
    public class LightBubble : EntityBubble
    {
        private const string ON_COLOR = "#FFE20F";
        private const string OFF_COLOR = "Transparent";

        private readonly IEntityService _entityService;
        private int _brightness;

        public LightBubble(Entity entity, IEntityService entityService)
         : base(entity)
        {
            if (entity.Type != "light")
                throw new ArgumentException("Entity has to be a light entity.");

            SupportsSlidingValue = true;

            _entityService = entityService;
            BackgroundOutline = ON_COLOR;
            Icon = "mdi-lightbulb";
        }

        protected override void OnEntityChanged()
        {
            BackgroundColor = Entity.State == "on" ? ON_COLOR : OFF_COLOR;

            var attr = Entity.Attributes.ToObject<LightAttributes>();
            _brightness = attr.Brightness;
            SlidingValue = _brightness / 255f;
        }

        public override async Task OnClickAsync()
        {
            Entity.UpdateWith(await _entityService.CallServiceAsync("light", "toggle", Entity.Id));
        }

        protected override async void OnSlidingValueChanged(float newValue)
        {
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