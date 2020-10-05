using System;
using System.Threading.Tasks;
using TouchBubbles.Client.Services;
using TouchBubbles.Shared.Models;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Client.ViewModels
{
    public class LightBubble : Bubble
    {
        private const string ON_COLOR = "#FFE20F";
        private const string OFF_COLOR = "Transparent";

        private Entity _entity;
        private readonly IEntityService _entityService;

        public LightBubble(Entity entity, IEntityService entityService)
        {
            if (entity.Type != "light")
                throw new ArgumentException("Entity has to be a light entity.");

            _entity = entity;
            _entityService = entityService;
        }

        public override string BackgroundOutline => ON_COLOR;

        public override string BackgroundColor => _entity.State == "on" ? ON_COLOR : OFF_COLOR;

        public override string Name => _entity.Name;

        public override async Task OnClickAsync()
        {
            _entity.UpdateWith(await _entityService.CallServiceAsync("light", "toggle", _entity.Id));
        }
    }
}