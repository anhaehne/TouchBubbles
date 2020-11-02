using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouchBubbles.Client.Services;
using TouchBubbles.Shared.Models;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Client.Models.Bubbles
{
    public class InputBooleanBubble : EntityBubble
    {
        private readonly IEntityService _entityService;

        public InputBooleanBubble(Entity entity, IEntityService entityService)
            : base(entity)
        {
            if (entity.Type != "input_boolean")
                throw new ArgumentException("Entity has to be a input_boolean entity.");
            _entityService = entityService;
        }

        public override async Task OnClickAsync()
        {
            Entity.State = Entity.State == "on" ? "off" : "on";
            await _entityService.CallServiceAsync("input_boolean", "toggle", Entity.Id);
        }

        public class Factory : IEntityBubbleFactory
        {
            private readonly IEntityService _entityService;

            public Factory(IEntityService entityService)
            {
                _entityService = entityService;
            }

            public string DefaultIcon => "mdi-lightbulb";

            public bool CanCreate(Entity entity) => entity.Type == "input_boolean";

            public EntityBubble Create(Entity entity) => new InputBooleanBubble(entity, _entityService);
        }
    }
}
