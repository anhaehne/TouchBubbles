using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TouchBubbles.Client.Models.Bubbles;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Client.Services
{
    public class BubbleFactory : IBubbleFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public BubbleFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public EntityBubble CreateBubble(Entity entity)
        {
            return GetFactory(entity)?.Create(entity) ?? throw new ArgumentException($"Couldn't find factory for entity type {entity.Type}.", nameof(entity));
        }

        public IEntityBubbleFactory? GetFactory(Entity entity)
        {
            var factories = _serviceProvider.GetServices<IEntityBubbleFactory>();

            return factories.FirstOrDefault(f => f.CanCreate(entity));
        }
    }
}