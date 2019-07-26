// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureSitecore.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.InPlaceUpgrade
{
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Customers;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;
    using System.Reflection;

    /// <summary>
    /// The Customers Upgrade configure class.
    /// </summary>
    /// <seealso cref="Sitecore.Framework.Configuration.IConfigureSitecore" />
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        /// The configure services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);
            services.RegisterAllCommands(assembly);

            services.Sitecore().Pipelines(config => config
                .AddPipeline<IUpgradeCustomerPipeline, UpgradeCustomerPipeline>(d =>
                {
                    d.Add<UpgradeCustomerBlock>()
                        .Add<PersistCustomerBlock>()
                        .Add<PersistCustomerIdIndexBlock>();
                })
                .AddPipeline<IUpgradeCommerceDataPipeline, UpgradeCommerceDataPipeline>(d =>
                {
                    d.Add<UpgradeCustomersBlock>();
                })

                .ConfigurePipeline<IConfigureOpsServiceApiPipeline>(configure => configure.Add<ConfigureOpsServiceApiBlock>())
            );
        }        
    }
}