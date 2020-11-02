using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reactive.Linq;
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
        private readonly IBubbleFactory _bubbleFactory;
        private readonly RangeObservableCollection<Entity> _entities = new RangeObservableCollection<Entity>();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly NavigationManager _navigationManager;
        private HubConnection? _hubConnection;
        private bool _isInitialized;

        public EntityService(IHttpClientFactory httpClientFactory, NavigationManager navigationManager,
            IServiceProvider serviceProvider)
        {
            _httpClientFactory = httpClientFactory;
            _navigationManager = navigationManager;

            // For some reason the bubble factory is not available during setup. This works, fix later. 
            _bubbleFactory = serviceProvider.GetRequiredService<IBubbleFactory>();
        }

        public void Dispose()
        {
            _ = _hubConnection?.DisposeAsync();
        }

        public IObservable<IReadOnlyCollection<Entity>> Entities => _entities
            .ToCollectionObservable()
            .Select(l => _entities.Where(TryUpdateEntity).ToList());

        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            _entities.AddRange(await GetEntitiesAsync());

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(new Uri(new Uri(_navigationManager.BaseUri), "homeassistant/hub"))
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<Entity>(
                "EntityUpdated",
                entity => UpdateEntities(new[] {entity}));

            await _hubConnection.StartAsync();

            _isInitialized = true;
        }

        public async Task CallServiceAsync(string domain, string service, string entityId)
        {
            var haClient = _httpClientFactory.CreateClient(EndPoints.BackEnd);

            var response = await haClient.PostAsJsonAsync(
                $"homeassistant/services/{domain}/{service}",
                new {entity_id = entityId});

            var updatedEntities = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<Entity>>() ??
                                  Array.Empty<Entity>();

            UpdateEntities(updatedEntities);
        }

        public async Task<JsonElement> CallServiceAsync<T>(string domain, string service, T data)
        {
            var haClient = _httpClientFactory.CreateClient(EndPoints.BackEnd);

            var response = await haClient.PostAsJsonAsync($"homeassistant/services/{domain}/{service}", data);

            return await response.Content.ReadFromJsonAsync<JsonElement>();
        }

        public async Task<IReadOnlyCollection<Entity>> GetEntitiesAsync()
        {
            var haClient = _httpClientFactory.CreateClient(EndPoints.BackEnd);

            var entities = await haClient.GetFromJsonAsync<IReadOnlyCollection<Entity>>("homeassistant/states") ??
                           Array.Empty<Entity>();

            return entities.ToList();
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
                var updatedEntity = _entities.SingleOrDefault(x => x.Id == entity.Id);

                if (updatedEntity is not null)
                    updatedEntity.UpdateWith(entity);
                else if (TryUpdateEntity(entity))
                    _entities.Add(entity);
            }
        }
    }
}