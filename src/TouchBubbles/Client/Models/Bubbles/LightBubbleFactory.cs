using TouchBubbles.Client.Services;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Client.Models.Bubbles
{
    public class LightBubbleFactory : IEntityBubbleFactory
    {
        private readonly IEntityService _entityService;

        public LightBubbleFactory(IEntityService entityService)
        {
            _entityService = entityService;
        }

        public string DefaultIcon => "mdi-lightbulb";

        public bool CanCreate(Entity entity) => entity.Type == "light";

        public EntityBubble Create(Entity entity) => new LightBubble(entity, _entityService);
    }
}
