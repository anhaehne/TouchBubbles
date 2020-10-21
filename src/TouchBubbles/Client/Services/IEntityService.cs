using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Client.Services
{
    public interface IEntityService
    {
        List<Entity> Entities { get; }

        Task CallServiceAsync(string domain, string service, string entityId);

        Task<JsonElement> CallServiceAsync<T>(string domain, string service, T data);

        Task InitializeAsync();
    }
}