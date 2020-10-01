using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HADotNet.Core;
using HADotNet.Core.Clients;
using Microsoft.Extensions.Options;
using TouchBubbles.Shared;
using TouchBubbles.Shared.Services;

namespace TouchBubbles.Server.Services
{
    public class EntityService : IEntityService
    {
        private readonly EntityClient _entityClient;

        public EntityService(IOptions<HomeAssistantOptions> haOptions)
        {
            _entityClient = ClientFactory.GetClient<EntityClient>();
        }

        public async Task<IReadOnlyCollection<string>> GetEntitiesAsync()
        {
            return (await _entityClient.GetEntities()).ToList();
        }
    }
}
