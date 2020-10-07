using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Server.Hubs
{
    public class HomeAssistantHub : Hub<IHomeAssistantHubClient>
    {
    }

    public interface IHomeAssistantHubClient
    {
        Task EntityUpdated(Entity updated);
    }
}