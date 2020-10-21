using TouchBubbles.Client.Models.Bubbles;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Client.Services
{
    public interface IBubbleFactory
    {
        EntityBubble CreateBubble(Entity entity);

        IEntityBubbleFactory? GetFactory(Entity entity);
    }
}