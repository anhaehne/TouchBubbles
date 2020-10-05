using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using TouchBubbles.Shared;
using TouchBubbles.Shared.Models;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Client.Services
{
    public class EntityService : IEntityService
    {
        private bool _isInitialized = false;
        private CancellationTokenSource _disposalTokenSource;
        ClientWebSocket _webSocket;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<HomeAssistantConfiguration> _haConfig;
        private long _id { get; set; } = 1;

        public List<Entity> Entities { get; set; } = new List<Entity>();
        public event Action EntityChanged;

        public EntityService(IHttpClientFactory httpClientFactory, IOptions<HomeAssistantConfiguration> haConfig)
        {
            _httpClientFactory = httpClientFactory;
            _haConfig = haConfig;
        }

        public async Task InitializeAsync()
        {
            if(_isInitialized)
                return;

            Entities = await GetEntitiesAsync();

            _disposalTokenSource = new CancellationTokenSource();
            _webSocket = new ClientWebSocket();

            var uriBuilder = new UriBuilder(new Uri(new Uri(_haConfig.Value.HomeAssistantApi), "api/websocket"));
            uriBuilder.Scheme = uriBuilder.Scheme == Uri.UriSchemeHttps ? "wss" : "ws";

            await _webSocket.ConnectAsync(
                uriBuilder.Uri,
                _disposalTokenSource.Token);

            _ = ReceiveLoop();

            _isInitialized = true;
        }

        public async Task<List<Entity>> GetEntitiesAsync()
        {
            var haClient = _httpClientFactory.CreateClient(Constants.HomeAssistant);

            var entities = await haClient.GetFromJsonAsync<List<Entity>>("api/states");

            return entities;
        }

        public async Task<Entity> CallServiceAsync(string domain, string service, string entityId)
        {
            var haClient = _httpClientFactory.CreateClient(Constants.HomeAssistant);

            var response = await haClient.PostAsJsonAsync($"api/services/{domain}/{service}", new { entity_id = entityId });

            return (await response.Content.ReadFromJsonAsync<IReadOnlyCollection<Entity>>()).SingleOrDefault(x => x.Id == entityId);
        }

        private async Task ReceiveLoop()
        {
            while (!_disposalTokenSource.IsCancellationRequested)
            {
                var received = await ReadNextEventAsync();
                await HandleMessageAsync(received);
            }
        }

        private async Task HandleMessageAsync(WebsocketApiMessage apiMessage)
        {
            switch (apiMessage.Type)
            {
                case "auth_required":
                    await SendResponseAsync(new
                    {
                        type = "auth",
                        access_token = _haConfig.Value.SupervisorToken,
                    });
                    break;
                case "auth_ok":
                    await SendResponseAsync(new
                    {
                        id = _id++,
                        type = "subscribe_events",
                        event_type = "state_changed",
                    });
                    break;
                case "event":
                    var apiEvent = apiMessage.Event.ToObject<WebsocketApiEvent>();
                    await HandleEventAsync(apiEvent);
                    break;
            }
        }
        private async Task HandleEventAsync(WebsocketApiEvent apiEvent)
        {
            switch (apiEvent.EventType)
            {
                case "state_changed":
                    var eventData = apiEvent.Data.ToObject<WebsocketApiStateChangedEventData>();
                    var entity = Entities.SingleOrDefault(x => x.Id == eventData.EntityId);
                    entity?.UpdateWith(eventData.NewState);
                    EntityChanged?.Invoke();
                    break;
            }
        }

        private async Task<WebsocketApiMessage> ReadNextEventAsync()
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            WebSocketReceiveResult result = null;

            await using var ms = new MemoryStream();

            do
            {
                result = await _webSocket.ReceiveAsync(buffer, _disposalTokenSource.Token);
                ms.Write(buffer.Array, buffer.Offset, result.Count);
            }
            while (!result.EndOfMessage);

            ms.Seek(0, SeekOrigin.Begin);

            return await JsonSerializer.DeserializeAsync<WebsocketApiMessage>(ms, cancellationToken: _disposalTokenSource.Token);
        }

        private async Task SendResponseAsync<T>(T response)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(response);
            await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, _disposalTokenSource.Token);
        }

        private class WebsocketApiMessage
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("event")]
            public JsonElement Event { get; set; }
        }

        private class WebsocketApiEvent
        {
            [JsonPropertyName("time_fired")]
            public DateTimeOffset TimeFired { get; set; }

            [JsonPropertyName("event_type")]
            public string EventType { get; set; }

            [JsonPropertyName("origin")]
            public string Origin { get; set; }

            [JsonPropertyName("data")]
            public JsonElement Data { get; set; }
        }

        private class WebsocketApiStateChangedEventData
        {
            [JsonPropertyName("entity_id")]
            public string EntityId { get; set; }

            [JsonPropertyName("new_state")]
            public Entity NewState { get; set; }

            [JsonPropertyName("old_state")]
            public Entity OldState { get; set; }
        }
    }
}