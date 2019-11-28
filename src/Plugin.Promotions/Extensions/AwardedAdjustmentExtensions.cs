using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Pricing;
using System.Collections.Generic;

namespace Promethium.Plugin.Promotions.Extensions
{
    public static class AwardedAdjustmentExtensions
    {
        internal static void AddCartLevelAwardedAdjustment(this IList<AwardedAdjustment> adjustments, CommerceContext context, decimal amountOff, string awardingBlock)
        {
            var propertiesModel = context.GetObject<PropertiesModel>();
            var discount = context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Discount;

            var adjustment = new CartLevelAwardedAdjustment {
                Name = propertiesModel?.GetPropertyValue("PromotionText") as string ?? discount,
                DisplayName = propertiesModel?.GetPropertyValue("PromotionCartText") as string ?? discount,
                Adjustment = new Money(context.CurrentCurrency(), amountOff),
                AdjustmentType = discount,
                IsTaxable = false,
                AwardingBlock = awardingBlock,
            };

            adjustments.Add(adjustment);
        }

        internal static void AddLineLevelAwardedAdjustment(this IList<AwardedAdjustment> adjustments, CommerceContext context, decimal amountOff, string awardingBlock, string lineItemId)
        {
            var propertiesModel = context.GetObject<PropertiesModel>();
            var discount = context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Discount;

            var adjustment = new CartLineLevelAwardedAdjustment() {
                Name = (propertiesModel?.GetPropertyValue("PromotionText") as string ?? discount),
                DisplayName = (propertiesModel?.GetPropertyValue("PromotionCartText") as string ?? discount),
                Adjustment = new Money(context.CurrentCurrency(), amountOff),
                AdjustmentType = discount,
                IsTaxable = false,
                AwardingBlock = awardingBlock,
                LineItemId = lineItemId,
            };

            adjustments.Add(adjustment);
        }
    }
}
