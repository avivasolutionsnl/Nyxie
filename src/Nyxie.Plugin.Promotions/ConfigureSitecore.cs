using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Nyxie.Plugin.Promotions.Pipelines.Blocks;
using Nyxie.Plugin.Promotions.Resolvers;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Rules;
using Sitecore.Commerce.Plugin.Search;
using Sitecore.Framework.Configuration;
using Sitecore.Framework.Pipelines.Definitions.Extensions;
using Sitecore.Framework.Rules;

using BuildRuleSetBlock = Nyxie.Plugin.Promotions.Pipelines.Blocks.BuildRuleSetBlock;
using RegisteredPluginBlock = Nyxie.Plugin.Promotions.Pipelines.Blocks.RegisteredPluginBlock;

namespace Nyxie.Plugin.Promotions
{
    /// <summary>
    ///     The configure sitecore class.
    /// </summary>
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
            services.RegisterAllCommands(assembly);

            services.AddTransient<CategoryPathResolver>();
            services.AddTransient<CategoryCartLinesResolver>();
            services.AddTransient<CategoryOrderLinesResolver>();
            services.AddTransient<OrderResolver>();

            services.Sitecore().Rules(rules => rules.Registry(reg => reg.RegisterAssembly(assembly)));

            services.Sitecore().Pipelines(config => config
                                                    .ConfigurePipeline<IAddCartLinePipeline>(configure => configure
                                                                                                          .Add<AddCategoryBlock>()
                                                                                                          .Before<PersistCartBlock
                                                                                                          >())

                                                    ////////////////////////////////////////////////////////////////////////////////////////////////////
                                                    .ConfigurePipeline<IGetEntityViewPipeline>(configure => configure
                                                                                                            .Add<
                                                                                                                ConditionDetailsView_CategoryBlock
                                                                                                            >()
                                                                                                            .After<
                                                                                                                GetPromotionQualificationDetailsViewBlock
                                                                                                            >())
                                                    .ConfigurePipeline<IGetEntityViewPipeline>(configure => configure
                                                                                                            .Add<
                                                                                                                ConditionDetailsView_FulfillmentBlock
                                                                                                            >()
                                                                                                            .After<
                                                                                                                ConditionDetailsView_CategoryBlock
                                                                                                            >())
                                                    .ConfigurePipeline<IGetEntityViewPipeline>(configure => configure
                                                                                                            .Add<
                                                                                                                ConditionDetailsView_PaymentBlock
                                                                                                            >()
                                                                                                            .After<
                                                                                                                ConditionDetailsView_FulfillmentBlock
                                                                                                            >())
                                                    .ConfigurePipeline<IGetEntityViewPipeline>(configure => configure
                                                                                                            .Add<
                                                                                                                ConditionDetailsView_BasicStringCompareBlock
                                                                                                            >()
                                                                                                            .After<
                                                                                                                ConditionDetailsView_PaymentBlock
                                                                                                            >())
                                                    .ConfigurePipeline<IGetEntityViewPipeline>(configure => configure
                                                                                                            .Add<
                                                                                                                ConditionDetailsView_ApplyActionTo
                                                                                                            >()
                                                                                                            .After<
                                                                                                                ConditionDetailsView_BasicStringCompareBlock
                                                                                                            >())

                                                    ////////////////////////////////////////////////////////////////////////////////////////////////////
                                                    .ConfigurePipeline<IGetEntityViewPipeline>(configure => configure
                                                                                                            .Add<
                                                                                                                PrettifyPromotionChildrenDetailsBlock
                                                                                                            >()
                                                                                                            .After<
                                                                                                                IFormatEntityViewPipeline
                                                                                                            >())

                                                    ////////////////////////////////////////////////////////////////////////////////////////////////////
                                                    .ConfigurePipeline<ISearchPipeline>(configure => configure
                                                                                                     .Add<
                                                                                                         ExtendCategorySearchResultBlock
                                                                                                     >()
                                                                                                     .After<
                                                                                                         IFormatEntityViewPipeline
                                                                                                     >())

                                                    ////////////////////////////////////////////////////////////////////////////////////////////////////
                                                    .ConfigurePipeline<IDoActionPipeline>(configure => configure
                                                                                                       .Add<
                                                                                                           ConditionDetailsView_CategoryBlock
                                                                                                       >()
                                                                                                       .After<
                                                                                                           DoActionSelectQualificationBlock
                                                                                                       >())
                                                    .ConfigurePipeline<IDoActionPipeline>(configure => configure
                                                                                                       .Add<
                                                                                                           ConditionDetailsView_FulfillmentBlock
                                                                                                       >()
                                                                                                       .After<
                                                                                                           ConditionDetailsView_CategoryBlock
                                                                                                       >())
                                                    .ConfigurePipeline<IDoActionPipeline>(configure => configure
                                                                                                       .Add<
                                                                                                           ConditionDetailsView_PaymentBlock
                                                                                                       >()
                                                                                                       .After<
                                                                                                           ConditionDetailsView_FulfillmentBlock
                                                                                                       >())
                                                    .ConfigurePipeline<IDoActionPipeline>(configure => configure
                                                                                                       .Add<
                                                                                                           ConditionDetailsView_BasicStringCompareBlock
                                                                                                       >()
                                                                                                       .After<
                                                                                                           ConditionDetailsView_PaymentBlock
                                                                                                       >())
                                                    .ConfigurePipeline<IDoActionPipeline>(configure => configure
                                                                                                       .Add<
                                                                                                           ConditionDetailsView_ApplyActionTo
                                                                                                       >()
                                                                                                       .After<
                                                                                                           ConditionDetailsView_BasicStringCompareBlock
                                                                                                       >())

                                                    ////////////////////////////////////////////////////////////////////////////////////////////////////
                                                    .ConfigurePipeline<IGetEntityViewPipeline>(configure => configure
                                                                                                            .Add<
                                                                                                                ConditionDetailsView_FulfillmentBlock
                                                                                                            >()
                                                                                                            .After<
                                                                                                                ConditionDetailsView_CategoryBlock
                                                                                                            >())
                                                    .ConfigurePipeline<IGetEntityViewPipeline>(configure => configure
                                                                                                            .Add<
                                                                                                                ConditionDetailsView_PaymentBlock
                                                                                                            >()
                                                                                                            .After<
                                                                                                                ConditionDetailsView_FulfillmentBlock
                                                                                                            >())
                                                    .ConfigurePipeline<IGetEntityViewPipeline>(configure => configure
                                                                                                            .Add<
                                                                                                                ConditionDetailsView_BasicStringCompareBlock
                                                                                                            >()
                                                                                                            .After<
                                                                                                                ConditionDetailsView_PaymentBlock
                                                                                                            >())

                                                    ////////////////////////////////////////////////////////////////////////////////////////////////////
                                                    .ConfigurePipeline<IDoActionPipeline>(configure => configure
                                                                                                       .Add<
                                                                                                           ConditionDetailsView_CategoryBlock
                                                                                                       >()
                                                                                                       .After<Sitecore.Commerce.
                                                                                                           Plugin.Promotions.
                                                                                                           DoActionSelectQualificationBlock
                                                                                                       >())
                                                    .ConfigurePipeline<IDoActionPipeline>(configure => configure
                                                                                                       .Add<
                                                                                                           ConditionDetailsView_FulfillmentBlock
                                                                                                       >()
                                                                                                       .After<
                                                                                                           ConditionDetailsView_CategoryBlock
                                                                                                       >())
                                                    .ConfigurePipeline<IDoActionPipeline>(configure => configure
                                                                                                       .Add<
                                                                                                           ConditionDetailsView_PaymentBlock
                                                                                                       >()
                                                                                                       .After<
                                                                                                           ConditionDetailsView_FulfillmentBlock
                                                                                                       >())
                                                    .ConfigurePipeline<IDoActionPipeline>(configure => configure
                                                                                                       .Add<
                                                                                                           ConditionDetailsView_BasicStringCompareBlock
                                                                                                       >()
                                                                                                       .After<
                                                                                                           ConditionDetailsView_PaymentBlock
                                                                                                       >())

                                                    ////////////////////////////////////////////////////////////////////////////////////////////////////
                                                    .ConfigurePipeline<IBuildRuleSetPipeline>(configure => configure
                                                                                                           .Remove<Sitecore.
                                                                                                               Commerce.Plugin.
                                                                                                               Rules.
                                                                                                               BuildRuleSetBlock>()
                                                                                                           .Add<BuildRuleSetBlock
                                                                                                           >())

                                                    ////////////////////////////////////////////////////////////////////////////////////////////////////
                                                    .ConfigurePipeline<IFormatEntityViewPipeline>(configure => configure
                                                                                                               .Add<
                                                                                                                   PrettifySelectOptionsBlock
                                                                                                               >()
                                                                                                               .After<
                                                                                                                   HighlightLocalizableViewPropertiesBlock
                                                                                                               >())

                                                    ////////////////////////////////////////////////////////////////////////////////////////////////////
                                                    .ConfigurePipeline<IRunningPluginsPipeline>(
                                                        c => c.Add<RegisteredPluginBlock>())
            );
        }
    }
}
