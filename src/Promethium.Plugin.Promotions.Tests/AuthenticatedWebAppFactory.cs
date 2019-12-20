using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;

using IdentityServer4.AccessTokenValidation;

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Promethium.Plugin.Promotions.Tests.Persistence;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Engine;
using Sitecore.Commerce.Plugin.Coupons;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Commerce.Plugin.SQL;
using Sitecore.Framework.Pipelines.Definitions.Extensions;

using Xunit.Abstractions;

using FindEntityBlock = Promethium.Plugin.Promotions.Tests.Persistence.Pipelines.Blocks.FindEntityBlock;

namespace Promethium.Plugin.Promotions.Tests
{
    public class AuthenticatedWebAppFactory : WebApplicationFactory<Startup>
    {
        private readonly string token;
        private readonly HttpMessageHandler identityServerHandler;
        private readonly Func<ITestOutputHelper> getTestOutputHelper;

        private InMemoryStore inMemoryStore = new InMemoryStore();
        private InMemoryListStore inMemoryListStore = new InMemoryListStore();

        public AuthenticatedWebAppFactory(string token, HttpMessageHandler identityServerHandler, Func<ITestOutputHelper> getTestOutputHelper)
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
                    //{ "Logging:LogLevel:Default", "Debug" },
                    //{ "Logging:PipelineTraceLoggingEnabled", "true" }
                });
            });

            base.ConfigureWebHost(builder);

            builder.UseSolutionRelativeContentRoot("")
                   .UseStartup<Startup>();

            builder.ConfigureLogging(c => { c.AddProvider(new XunitLoggerProvider(getTestOutputHelper())); });
            builder.ConfigureServices(c => {
                c.Configure<IdentityServerAuthenticationOptions>("Bearer", options =>
                {
                    options.Authority = "http://localhost";

                    // IMPORTANT PART HERE
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

                var assembly = Assembly.GetExecutingAssembly();
                services.RegisterAllPipelineBlocks(assembly);

                services.Sitecore().Pipelines(config => config
                                                        //.ConfigurePipeline<IPersistEntityPipeline>(c => c.Add<PersistEntityBlock>().Before<StoreEntityInMemoryCacheBlock>())
                                                        .ConfigurePipeline<IFindEntitiesInListPipeline>(c =>
                                                        {
                                                            c.Clear();
                                                            c.Add<Persistence.Pipelines.Blocks.FindEntitiesInListBlock>();
                                                        })
                                                        .ConfigurePipeline<IFindEntityPipeline>(c =>
                                                        {
                                                            c.Clear();
                                                            c.Add<FindEntityBlock>();
                                                        })
                                                        .ConfigurePipeline<IDiscoverPromotionsPipeline>(c =>
                                                        {
                                                            c.Replace<FilterPromotionsByValidDateBlock, Persistence.Pipelines.Blocks.
                                                                LoggingBlock<FilterPromotionsByValidDateBlock>>();
                                                            c.Replace<FilterNotApprovedPromotionsBlock, Persistence.Pipelines.Blocks.
                                                                LoggingBlock<FilterNotApprovedPromotionsBlock>>();
                                                            c.Replace<FilterPromotionsByItemsBlock, Persistence.Pipelines.Blocks.
                                                                LoggingBlock<FilterPromotionsByItemsBlock>>();
                                                            c.Replace<FilterPromotionsByBookAssociatedCatalogsBlock, Persistence.Pipelines.Blocks.
                                                                LoggingBlock<FilterPromotionsByBookAssociatedCatalogsBlock>>();
                                                            c.Replace<FilterPromotionsByBenefitTypeBlock, Persistence.Pipelines.Blocks.
                                                                LoggingBlock<FilterPromotionsByBenefitTypeBlock>>();
                                                            c.Replace<FilterPromotionsByCouponBlock, Persistence.Pipelines.Blocks.
                                                                LoggingBlock<FilterPromotionsByCouponBlock>>();
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
            {
                inMemoryStore.Add(entity);
            }
        }
        public void AddEntityToList(CommerceEntity entity, string list)
        {
            inMemoryListStore.Add(list, entity);
        }

        public void AddEntitiesToList(string list, params CommerceEntity[] entities)
        {
            foreach (CommerceEntity entity in entities)
            {
                inMemoryListStore.Add(list, entity);
            }
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
                    ConnectionId = Guid.NewGuid().ToString("N", (IFormatProvider)CultureInfo.InvariantCulture),
                    CorrelationId = Guid.NewGuid().ToString("N", (IFormatProvider)CultureInfo.InvariantCulture),
                    TrackActivityPipeline = service,
                    PipelineTraceLoggingEnabled = true
                };
            }
        }
    }
}
