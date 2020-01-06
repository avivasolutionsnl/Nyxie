// © 2016 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Plugin.Sample.Habitat.Pipelines.Blocks;

using Sitecore.Commerce.Core;
using Sitecore.Framework.Configuration;
using Sitecore.Framework.Pipelines.Definitions.Extensions;

namespace Plugin.Sample.Habitat
{
    /// <summary>
    ///     The Habitat configure class.
    /// </summary>
    /// <seealso cref="IConfigureSitecore" />
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        ///     The configure services.
        /// </summary>
        /// <param name="services">
        ///     The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(
                config =>
                    config
                        .ConfigurePipeline<IInitializeEnvironmentPipeline>(
                            d =>
                            {
                                d.Add<InitializeCatalogBlock>()
                                 .Add<InitializeEnvironmentSellableItemsBlock>()
                                 .Add<InitializeEnvironmentBundlesBlock>()
                                 .Add<InitializeInventoryBlock>()
                                 .Add<InitializeEnvironmentPricingBlock>()
                                 .Add<InitializeEnvironmentPromotionsBlock>();
                            })
                        .ConfigurePipeline<IRunningPluginsPipeline>(c =>
                        {
                            c.Add<RegisteredPluginBlock>().After<RunningPluginsBlock>();
                        }));
        }
    }
}
