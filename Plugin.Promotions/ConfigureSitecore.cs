// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureSitecore.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Commerce.Plugin.Sample
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Commerce.Core;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;
    using Sitecore.Commerce.Plugin.Carts;
    using global::Plugin.Promotions.Pipelines.Blocks;
    using Sitecore.Framework.Rules;
    using Sitecore.Commerce.EntityViews;

    /// <summary>
    /// The configure sitecore class.
    /// </summary>
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

            services.Sitecore().Pipelines(config => config             
               .ConfigurePipeline<IAddCartLinePipeline>(configure => configure.Add<AddCategoryBlock>().Before<PersistCartBlock>())
               .ConfigurePipeline<IGetEntityViewPipeline>(configure => configure.Replace<Promotions.GetPromotionQualificationDetailsViewBlock, GetPromotionQualificationDetailsViewBlock>())
               );

            services.Sitecore().Rules(rules => rules.Registry(reg => reg.RegisterThisAssembly()));

            services.RegisterAllCommands(assembly);
        }
    }
}