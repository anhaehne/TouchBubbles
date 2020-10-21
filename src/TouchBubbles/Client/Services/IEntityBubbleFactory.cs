using TouchBubbles.Client.Models.Bubbles;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Client.Services
{
    public interface IEntityBubbleFactory
    {
        string DefaultIcon { get; }

        bool CanCreate(Entity entity);

        EntityBubble Create(Entity entity);
    }
}