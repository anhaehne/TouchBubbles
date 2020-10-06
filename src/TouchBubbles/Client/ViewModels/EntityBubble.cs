using TouchBubbles.Shared.Models;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Client.ViewModels
{
    public class EntityBubble : Bubble
    {
        public EntityBubble(Entity entity)
        {
            Entity = entity;
            Name = Entity.Name;
            Entity.EntityChanged += OnEntityChangedInternal;
            OnEntityChanged();
        }

        private void OnEntityChangedInternal()
        {
            OnEntityChanged();
            InvokeBubbleChanged();
        }

        public Entity Entity { get; }

        protected virtual void OnEntityChanged() { }
    }
}