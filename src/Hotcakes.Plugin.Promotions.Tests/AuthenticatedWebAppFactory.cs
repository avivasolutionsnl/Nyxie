using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;

using Hotcakes.Plugin.Promotions.Tests.Persistence;

using IdentityServer4.AccessTokenValidation;

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Engine;
using Sitecore.Commerce.Plugin.SQL;
using Sitecore.Framework.Pipelines.Definitions.Extensions;

using Xunit.Abstractions;

using FindEntitiesInListBlock = Hotcakes.Plugin.Promotions.Tests.Persistence.Pipelines.Blocks.FindEntitiesInListBlock;
using FindEntityBlock = Hotcakes.Plugin.Promotions.Tests.Persistence.Pipelines.Blocks.FindEntityBlock;

namespace Hotcakes.Plugin.Promotions.Tests
{
    public class AuthenticatedWebAppFactory : WebApplicationFactory<Startup>
    {
        private readonly Func<ITestOutputHelper> getTestOutputHelper;
        private readonly HttpMessageHandler identityServerHandler;
        private readonly string token;
        private readonly InMemoryListStore inMemoryListStore = new InMemoryListStore();

        private readonly InMemoryStore inMemoryStore = new InMemoryStore();

        public AuthenticatedWebAppFactory(string token, HttpMessageHandler identityServerHandler,
            Func<ITestOutputHelper> getTestOutputHelper)
        {
            this.token = token;
            this.identityServerHandler = identityServerHandler;
            this.getTestOutputHelper = getTestOutputHelper;
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseWebRoot(Path.GetFullPath("wwwroot"));

            builder.ConfigureAppConfiguration((context, b) =>
            {
                b.SetBasePath(Path.GetFullPath("wwwroot"))
                 .AddJsonFile("config.json", false, true);
                b.AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "AppSettings:SitecoreIdentityServerUrl", "http://localhost" },
                    { "Logging:LogLevel:Default", "Debug" },
                    { "Logging:PipelineTraceLoggingEnabled", "true" }
                });
            });

            base.ConfigureWebHost(builder);

            builder.UseSolutionRelativeContentRoot("")
                   .UseStartup<Startup>();

            builder.ConfigureLogging(c => { c.AddProvider(new XunitLoggerProvider(getTestOutputHelper())); });
            builder.ConfigureServices(c =>
            {
                c.Configure<IdentityServerAuthenticationOptions>("Bearer", options =>
                {
                    options.Authority = "http://localhost";

                    // This makes sure the commerce engine contacts the in memory identity server
                    options.JwtBackChannelHandler = identityServerHandler;
                    options.IntrospectionDiscoveryHandler = identityServerHandler;
                    options.IntrospectionBackChannelHandler = identityServerHandler;
                });
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddTransient<GetDatabaseVersionCommand, DummyGetDatabaseVersionCommand>();

                /************************************** Persistent ***********************************/
                services.AddSingleton<IStore>(inMemoryStore);
                services.AddSingleton(inMemoryStore);

                services.AddSingleton<IListStore>(inMemoryListStore);
                services.AddSingleton(inMemoryListStore);

                Assembly assembly = Assembly.GetExecutingAssembly();
                services.RegisterAllPipelineBlocks(assembly);

                services.Sitecore().Pipelines(config => config.ConfigurePipeline<IFindEntitiesInListPipeline>(c =>
                                                              {
                                                                  c.Clear();
                                                                  c.Add<FindEntitiesInListBlock>();
                                                              })
                                                              .ConfigurePipeline<IFindEntityPipeline>(c =>
                                                              {
                                                                  c.Clear();
                                                                  c.Add<FindEntityBlock>();
                                                              }));
            });
        }

        protected override void ConfigureClient(HttpClient client)
        {
            client.SetBearerToken(token);
        }

        public void AddEntity(CommerceEntity entity)
        {
            inMemoryStore.Add(entity);
        }

        public void AddEntities(params CommerceEntity[] entities)
        {
            foreach (CommerceEntity entity in entities)
                inMemoryStore.Add(entity);
        }

        public void AddEntityToList(CommerceEntity entity, string list)
        {
            inMemoryListStore.Add(list, entity);
        }

        public void AddEntitiesToList(string list, params CommerceEntity[] entities)
        {
            foreach (CommerceEntity entity in entities)
                inMemoryListStore.Add(list, entity);
        }

        public void ClearAllEntities()
        {
            inMemoryStore.Entities.Clear();
            inMemoryListStore.Lists.Clear();
        }

        public CommerceContext CreateCommerceContext()
        {
            using (IServiceScope scope = Server.Host.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetService<ILogger<CommerceController>>();
                var messagePipeline = scope.ServiceProvider.GetService<IGetLocalizableMessagePipeline>();
                var service = scope.ServiceProvider.GetService<ITrackActivityPipeline>();
                var globalEnvironment = scope.ServiceProvider.GetService<CommerceEnvironment>();
                return new CommerceContext(logger, new TelemetryClient(), messagePipeline)
                {
                    GlobalEnvironment = globalEnvironment,
                    Environment = globalEnvironment,
                    ConnectionId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
                    CorrelationId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
                    TrackActivityPipeline = service,
                    PipelineTraceLoggingEnabled = true
                };
            }
        }
    }
}
