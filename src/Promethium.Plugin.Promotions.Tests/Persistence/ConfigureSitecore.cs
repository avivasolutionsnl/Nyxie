using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Promethium.Plugin.Promotions.Tests.Persistence.Pipelines.Blocks;

using Sitecore.Commerce.Core;
using Sitecore.Framework.Configuration;
using Sitecore.Framework.Pipelines.Definitions.Extensions;

namespace Promethium.Plugin.Promotions.Tests.Persistence
{
    public class ConfigureSitecore : IConfigureSitecore
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var inMemoryStore = new InMemoryStore();
            services.AddSingleton<IStore>(inMemoryStore);
            services.AddSingleton(inMemoryStore);

            var inMemoryListStore = new InMemoryListStore();
            services.AddSingleton<IListStore>(inMemoryListStore);
            services.AddSingleton(inMemoryListStore);

            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config
                                                    //.ConfigurePipeline<IPersistEntityPipeline>(c => c.Add<PersistEntityBlock>().Before<StoreEntityInMemoryCacheBlock>())
                                                    .ConfigurePipeline<IFindEntitiesInListPipeline>(c =>
                                                    {
                                                        c.Clear();
                                                        c.Add<Pipelines.Blocks.FindEntitiesInListBlock>();
                                                    })
                                                    .ConfigurePipeline<IFindEntityPipeline>(c =>
                                                    {
                                                        c.Clear();
                                                        c.Add<FindEntityBlock>();
                                                    })
            );

            services.RegisterAllCommands(assembly);
        }
    }
}
