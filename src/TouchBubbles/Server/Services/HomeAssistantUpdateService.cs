using System;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TouchBubbles.Server.Hubs;
using TouchBubbles.Shared.Models;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Server.Services
{
    public class HomeAssistantUpdateService : IHostedService
    {
        private readonly ILogger<HomeAssistantUpdateService> _logger;
        private readonly IOptions<HomeAssistantConfiguration> _haConfig;
        private readonly IHubContext<HomeAssistantHub, IHomeAssistantHubClient> _haHubContext;
        private CancellationTokenSource _cancellationTokenSource;
        private long _id;
        private Task _receiveTask;
        private ClientWebSocket _webSocket;

        public HomeAssistantUpdateService(
            ILogger<HomeAssistantUpdateService> logger,
            IOptions<HomeAssistantConfiguration> haConfig,
            IHubContext<HomeAssistantHub, IHomeAssistantHubClient> haHubContext)
        {
            _logger = logger;
            _haConfig = haConfig;
            _haHubContext = haHubContext;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _webSocket = new ClientWebSocket();
            _id = 1;

            var uriBuilder = new UriBuilder(new Uri(new Uri(_haConfig.Value.HomeAssistantApi), "api/websocket"));
            uriBuilder.Scheme = uriBuilder.Scheme == Uri.UriSchemeHttps ? "wss" : "ws";

            await _webSocket.ConnectAsync(uriBuilder.Uri, cancellationToken);

            _logger.LogInformation("Connected to home assistant websocket api.");

            _receiveTask = ReceiveAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            await _receiveTask;
        }

        private async Task ReceiveAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
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
                    await SendResponseAsync(
                        new
                        {
                            type = "auth",
                            access_token = _haConfig.Value.SupervisorToken
                        });

                    break;
                case "auth_ok":
                    await SendResponseAsync(
                        new
                        {
                            id = _id++,
                            type = "subscribe_events",
                            event_type = "state_changed"
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

                    await _haHubContext.Clients.All.EntityUpdated(eventData.NewState);

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
                result = await _webSocket.ReceiveAsync(buffer, _cancellationTokenSource.Token);
                ms.Write(buffer.Array, buffer.Offset, result.Count);
            } while (!result.EndOfMessage);

            ms.Seek(0, SeekOrigin.Begin);

            return await JsonSerializer.DeserializeAsync<WebsocketApiMessage>(
                ms,
                cancellationToken: _cancellationTokenSource.Token);
        }

        private async Task SendResponseAsync<T>(T response)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(response);
            await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
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