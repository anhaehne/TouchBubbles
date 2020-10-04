using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TouchBubbles.Shared;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Client.Services
{
    public interface IEntityService
    {
        Task<IReadOnlyCollection<Entity>> GetEntitiesAsync();

        Task<Entity> CallServiceAsync(string domain, string service, string entityId);
    }

    public class EntityService : IEntityService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EntityService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IReadOnlyCollection<Entity>> GetEntitiesAsync()
        {
            var haClient = _httpClientFactory.CreateClient(Constants.HomeAssistant);

            var entities = await haClient.GetFromJsonAsync<Entity[]>("api/states");

            return entities;
        }

        public async Task<Entity> CallServiceAsync(string domain, string service, string entityId)
        {
            var haClient = _httpClientFactory.CreateClient(Constants.HomeAssistant);

            var response = await haClient.PostAsJsonAsync($"api/services/{domain}/{service}", new { entity_id = entityId });

            return (await response.Content.ReadFromJsonAsync<IReadOnlyCollection<Entity>>()).SingleOrDefault(x => x.Id == entityId);
        }
    }
}
