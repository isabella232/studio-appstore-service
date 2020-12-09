using AppStoreIntegrationService.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using System;
using static AppStoreIntegrationService.Enums;
using AppStoreIntegrationService.Model;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System.Net.Http;
using AppStoreIntegrationService.Controllers;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Read the deploy mode from appsettings.json
            var settingsDeployMode = Configuration.GetValue<string>("DeployMode");
            Enum.TryParse(settingsDeployMode, out DeployMode deployMode);

            //Read the web host enviroment in case the service is set on LocalFilePath
            //we'll need to read the json file with the plugins details from local path
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var env = serviceProvider.GetService<IWebHostEnvironment>();

            var configurationSettings = new ConfigurationSettings();

            //Read from appsettings if exists 
            var section = Configuration.GetSection("ConfigurationSettings");
            if (section.Exists())
            {
                Configuration.Bind("ConfigurationSettings", configurationSettings);
            }
            else
            {
                //Read from environment variables
                configurationSettings.LoadVariables();
            }

            services.AddMvc();

            services.AddHttpContextAccessor();
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            if (!string.IsNullOrEmpty(configurationSettings.InstrumentationKey))
            {
                services.AddApplicationInsightsTelemetry();
                new TelemetryClient(new TelemetryConfiguration(configurationSettings.InstrumentationKey)).TrackEvent("Application started");
            }

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.AddResponseCaching();
            services.AddTransient<PluginsController, PluginsController>();
            services.AddTransient<CategoriesController, CategoriesController>();
            services.AddHttpContextAccessor();

            //All the methods related to Azure blob (creates or read for release/demo deploy mode the files with plugins details which we'll be displayed in Studio
            var azureRepository = new AzureRepository(deployMode, configurationSettings);
            var pluginRepository = new PluginRepository(azureRepository, env, configurationSettings);

            services.AddSingleton<IAzureRepository>(azureRepository);
            services.AddSingleton<IPluginRepository>(pluginRepository);

            services.AddHttpClient("HttpClientWithSSLUntrusted").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
            (httpRequestMessage, cert, cetChain, policyErrors) =>
            {
                return true;
            }
            });

            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseResponseCaching();

            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromMinutes(9),

                    };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
                    new[] { "Accept-Encoding" };

                await next();
            });

            app.UseResponseCompression();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
