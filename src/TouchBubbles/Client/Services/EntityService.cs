using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using TouchBubbles.Shared;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Client.Services
{
    public class EntityService : IEntityService, IDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly NavigationManager _navigationManager;
        private HubConnection _hubConnection;
        private bool _isInitialized;

        public EntityService(IHttpClientFactory httpClientFactory, NavigationManager navigationManager)
        {
            _httpClientFactory = httpClientFactory;
            _navigationManager = navigationManager;
        }

        public void Dispose()
        {
            _ = _hubConnection.DisposeAsync();
        }

        public List<Entity> Entities { get; set; } = new List<Entity>();

        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            Entities = await GetEntitiesAsync();

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(new Uri(new Uri(_navigationManager.BaseUri), "homeassistant/hub"))
                .Build();

            _hubConnection.On<Entity>(
                "EntityUpdated",
                entity =>
                {
                    var updatedEntity = Entities.SingleOrDefault(x => x.Id == entity.Id);

                    if(updatedEntity != null)
                        updatedEntity?.UpdateWith(entity);
                    else
                        Entities.Add(entity);
                });

            await _hubConnection.StartAsync();

            _isInitialized = true;
        }

        public async Task<Entity> CallServiceAsync(string domain, string service, string entityId)
        {
            var haClient = _httpClientFactory.CreateClient(EndPoints.BackEnd);

            var response = await haClient.PostAsJsonAsync(
                $"homeassistant/services/{domain}/{service}",
                new { entity_id = entityId });

            return (await response.Content.ReadFromJsonAsync<IReadOnlyCollection<Entity>>()).SingleOrDefault(
                x => x.Id == entityId);
        }

        public async Task<JsonElement> CallServiceAsync<T>(string domain, string service, T data)
        {
            var haClient = _httpClientFactory.CreateClient(EndPoints.BackEnd);

            var response = await haClient.PostAsJsonAsync($"homeassistant/services/{domain}/{service}", data);

            return await response.Content.ReadFromJsonAsync<JsonElement>();
        }

        public async Task<List<Entity>> GetEntitiesAsync()
        {
            var haClient = _httpClientFactory.CreateClient(EndPoints.BackEnd);

            var entities = await haClient.GetFromJsonAsync<List<Entity>>("homeassistant/states");

            return entities;
        }
    }
}