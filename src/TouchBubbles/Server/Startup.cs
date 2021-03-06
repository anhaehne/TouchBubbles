using System;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TouchBubbles.Server.Hubs;
using TouchBubbles.Server.Services;
using TouchBubbles.Server.Utils;
using TouchBubbles.Shared;
using TouchBubbles.Shared.Models;

namespace TouchBubbles.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddHttpClient();

            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });

            services.AddHttpClient(EndPoints.HomeAssistant);

            services.AddHttpClient(
                EndPoints.HomeAssistant,
                (services, client) =>
                {
                    var haConfig = services.GetRequiredService<IOptions<HomeAssistantConfiguration>>().Value;
                    client.BaseAddress = new Uri(haConfig.HomeAssistantApi);

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        haConfig.SupervisorToken);
                });

            services.AddTransient<IProfileService, ProfileService>();
            services.AddTransient<IHomeAssistantService, HomeAssistantService>();
            services.AddHostedService<HomeAssistantUpdateService>();

            services.Configure<HomeAssistantConfiguration>(
                haOptions =>
                {
                    haOptions.HomeAssistantApi = Configuration["HOME_ASSISTANT_API"] ?? "http://supervisor/core/";
                    haOptions.SupervisorToken = Configuration["SUPERVISOR_TOKEN"];
                    haOptions.DataDirectory = Configuration["DATA_DIRECTORY"] ?? "/data";
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<AcceptEncodingMiddleware>();

            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                    context.Context.Response.Headers.Add("Expires", "-1");
                }
            });

            app.UseRouting();

            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapRazorPages();
                    endpoints.MapControllers();
                    endpoints.MapHub<HomeAssistantHub>("/homeassistant/hub");
                    endpoints.MapFallbackToFile("index.html");
                });
        }
    }
}