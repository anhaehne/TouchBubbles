using TouchBubbles.Shared.Models;
using TouchBubbles.Shared.Models.HomeAssistant;
using TouchBubbles.Shared.Utils;

namespace TouchBubbles.Client.Models.Bubbles
{
    public class EntityBubble : Bubble
    {
        public EntityBubble(Entity entity, string? backgroundColorOn = null)
        {
            BackgroundColor = backgroundColorOn ?? ColorHash.HEX(entity.Id);
            BackgroundColorOutline = BackgroundColor;
            Entity = entity;
            Name = entity.Name;
            Icon = entity.Icon;
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