using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TouchBubbles.Client.Services;
using TouchBubbles.Shared;
using TouchBubbles.Shared.Models;

namespace TouchBubbles.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            var haConfig = await GetConfigurationAsync(builder.HostEnvironment.BaseAddress);
            builder.Services.Configure<HomeAssistantConfiguration>(
                cur =>
                {
                    cur.SupervisorToken = haConfig.SupervisorToken;
                    cur.HomeAssistantApi = haConfig.HomeAssistantApi;
                });

            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient(
                Constants.BackEnd,
                client => { client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress); });

            builder.Services.AddHttpClient(
                Constants.HomeAssistant,
                client =>
                {
                    client.BaseAddress = new Uri(haConfig.HomeAssistantApi);

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        haConfig.SupervisorToken);
                });

            builder.Services.AddSingleton<IEntityService, EntityService>();

            await builder.Build().RunAsync();
        }

        private static async Task<HomeAssistantConfiguration> GetConfigurationAsync(string backendUrl)
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri(backendUrl) };

            return await httpClient.GetFromJsonAsync<HomeAssistantConfiguration>("Config");
        }
    }
}