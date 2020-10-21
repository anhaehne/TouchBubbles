using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TouchBubbles.Client.Models.Bubbles;
using TouchBubbles.Client.Services;
using TouchBubbles.Shared;

namespace TouchBubbles.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddHttpClient();

            builder.Services.AddHttpClient(
                EndPoints.BackEnd,
                client => { client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress); });

            // register bubble factories
            builder.Services.AddSingleton<IBubbleFactory, BubbleFactory>();
            builder.Services.AddSingleton<IEntityBubbleFactory, LightBubbleFactory>();

            builder.Services.AddSingleton<IEntityService, EntityService>();
            builder.Services.AddSingleton<IOverlayService, OverlayService>();
            builder.Services.AddSingleton<IProfileService, ProfileService>();

            await builder.Build().RunAsync();
        }
    }
}