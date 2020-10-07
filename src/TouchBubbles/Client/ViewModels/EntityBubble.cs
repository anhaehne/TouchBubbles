using TouchBubbles.Shared.Models;
using TouchBubbles.Shared.Models.HomeAssistant;
using TouchBubbles.Shared.Utils;

namespace TouchBubbles.Client.ViewModels
{
    public class EntityBubble : Bubble
    {
        public EntityBubble(Entity entity, string backgroundColorOn = null)
        {
            BackgroundColor = backgroundColorOn ?? ColorHash.HEX(entity.Id);
            Entity = entity;
            Name = Entity.Name;
            Entity.EntityChanged += () =>
            {
                OnEntityChangedInternal();
                InvokeBubbleChanged();
            };

            OnEntityChangedInternal();
        }

        private void OnEntityChangedInternal()
        {
            IsActive = Entity.State == "on";
            OnEntityChanged();
        }

        public Entity Entity { get; }

        protected virtual void OnEntityChanged() { }
    }
}