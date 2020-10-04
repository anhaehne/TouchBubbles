using System;
using System.Net.Http.Headers;
using HADotNet.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TouchBubbles.Client.Services;
using TouchBubbles.Shared;
using TouchBubbles.Shared.Models;
using TouchBubbles.Shared.Services;
using HomeAssistantConfigurationService = TouchBubbles.Server.Services.HomeAssistantConfigurationService;

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
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddHttpClient();

            services.AddHttpClient(Constants.HomeAssistant);
            services.AddHttpClient(
                Constants.HomeAssistant,
                (services, client )=>
                {
                    var haConfig = services.GetRequiredService<IOptions<HomeAssistantConfiguration>>().Value;
                    client.BaseAddress = new Uri(haConfig.HomeAssistantApi);

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        haConfig.SupervisorToken);
                });

            services.AddTransient<IEntityService, EntityService>();
            services.AddTransient<IHomeAssistantConfigurationService, HomeAssistantConfigurationService>();

            services.Configure<HomeAssistantConfiguration>(
                haOptions =>
                {
                    haOptions.HomeAssistantApi = Configuration["HOME_ASSISTANT_API"] ?? "http://supervisor/core";
                    haOptions.SupervisorToken = Configuration["SUPERVISOR_TOKEN"];
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapRazorPages();
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("index.html");
                });
        }
    }
}