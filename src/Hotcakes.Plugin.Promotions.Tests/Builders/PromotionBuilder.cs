using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Promotions;

namespace Hotcakes.Plugin.Promotions.Tests.Builders
{
    public class PromotionBuilder
    {
        private IBenefitBuilder[] benefitBuilders;
        private IQualificationBuilder[] qualificationBuilders;

        public PromotionBuilder QualifiedBy(params IQualificationBuilder[] qualificationBuilders)
        {
            this.qualificationBuilders = qualificationBuilders;
            return this;
        }

        public PromotionBuilder BenefitBy(params IBenefitBuilder[] benefitBuilders)
        {
            this.benefitBuilders = benefitBuilders;
            return this;
        }

        public async Task<Promotion> Build(AuthenticatedWebAppFactory factory)
        {
            var promotion = new Promotion
            {
                Id = "P001",
                ValidFrom = DateTimeOffset.UtcNow.AddDays(-10),
                ValidTo = DateTimeOffset.UtcNow.AddDays(10),
                IsPersisted = true,
                Published = true,
                Name = "My promotion"
            };

            promotion.AddPolicies(new PromotionQualificationsPolicy
            {
                Qualifications = qualificationBuilders.Select(x => x.Build()).ToList()
            }, new PromotionBenefitsPolicy
            {
                Benefits = benefitBuilders.Select(x => x.Build()).ToList()
            });

            promotion.AddComponents(new PromotionRulesComponent(), new ApprovalComponent("Approved"));

            using (IServiceScope scope = factory.Server.Host.Services.CreateScope())
            {
                var block = scope.ServiceProvider.GetRequiredService<BuildPromotionQualifyingRuleBlock>();
                promotion = await block.Run(promotion, factory.CreateCommerceContext().PipelineContext);

                var applyingBlock = scope.ServiceProvider.GetRequiredService<BuildPromotionApplyingRuleBlock>();
                promotion = await applyingBlock.Run(promotion, factory.CreateCommerceContext().PipelineContext);
            }

            return promotion;
        }
    }
}
