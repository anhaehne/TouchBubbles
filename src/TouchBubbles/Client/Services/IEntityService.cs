using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouchBubbles.Shared.Models.HomeAssistant;
using System.Text;
using System.Text.Json.Serialization;
using TouchBubbles.Shared.Services;

namespace TouchBubbles.Client.Services
{
    public interface IEntityService
    {
        Task<Entity> CallServiceAsync(string domain, string service, string entityId);

        Task InitializeAsync();

        List<Entity> Entities { get; set; }

        event Action EntityChanged;
    }
}
