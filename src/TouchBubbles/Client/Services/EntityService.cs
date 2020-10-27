using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using TouchBubbles.Shared;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Client.Services
{
    public class EntityService : IEntityService, IDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly NavigationManager _navigationManager;
        private readonly IBubbleFactory _bubbleFactory;
        private HubConnection? _hubConnection;
        private bool _isInitialized;

        public EntityService(IHttpClientFactory httpClientFactory, NavigationManager navigationManager, IServiceProvider serviceProvider)
        {
            _httpClientFactory = httpClientFactory;
            _navigationManager = navigationManager;

            // For some reason the bubble factory is not available during setup. This works, fix later. 
            _bubbleFactory = serviceProvider.GetService<IBubbleFactory>(); 
        }

        public void Dispose()
        {
            _ = _hubConnection?.DisposeAsync();
        }

        public List<Entity> Entities { get; private set; } = new List<Entity>();

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
                entity => UpdateEntities(new []{entity}));

            await _hubConnection.StartAsync();

            _isInitialized = true;
        }

        public async Task CallServiceAsync(string domain, string service, string entityId)
        {
            var haClient = _httpClientFactory.CreateClient(EndPoints.BackEnd);

            var response = await haClient.PostAsJsonAsync(
                $"homeassistant/services/{domain}/{service}",
                new { entity_id = entityId });

            var updatedEntities = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<Entity>>();

            UpdateEntities(updatedEntities);
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

            return Filter(entities).ToList();

            IEnumerable<Entity> Filter(IEnumerable<Entity> innerEntities)
            {
                foreach (var entity in innerEntities)
                {
                    if(TryUpdateEntity(entity))
                        yield return entity;
                }
            }
        }

        private bool TryUpdateEntity(Entity entity)
        {
            var factory = _bubbleFactory.GetFactory(entity);

            if (factory == null)
                return false;

            if (entity.Icon == "mdi-puzzle")
                entity.Icon = factory.DefaultIcon;

            return true;
        }

        private void UpdateEntities(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                var updatedEntity = Entities.SingleOrDefault(x => x.Id == entity.Id);

                if (updatedEntity != null)
                    updatedEntity?.UpdateWith(entity);
                else if(TryUpdateEntity(entity))
                    Entities.Add(entity);
            }
        }
    }
}