using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Pricing;

namespace Promethium.Plugin.Promotions
{
    public class AwardedAdjustmentFactory
    {
        public static CartLevelAwardedAdjustment CreateCartLevelAwardedAdjustment(decimal amountOff, string awardingBlock, CommerceContext commerceContext)
        {
            var propertiesModel = commerceContext.GetObject<PropertiesModel>();
            var discount = commerceContext.GetPolicy<KnownCartAdjustmentTypesPolicy>().Discount;

            var adjustment = new CartLevelAwardedAdjustment
            {
                Name = propertiesModel?.GetPropertyValue("PromotionText") as string ?? discount,
                DisplayName = propertiesModel?.GetPropertyValue("PromotionCartText") as string ?? discount,
                Adjustment = new Money(commerceContext.CurrentCurrency(), amountOff),
                AdjustmentType = discount,
                IsTaxable = false,
                AwardingBlock = awardingBlock,
            };

            return adjustment;
        }

        public static CartLineLevelAwardedAdjustment CreateLineLevelAwardedAdjustment(decimal amountOff, string awardingBlock, string lineItemId, CommerceContext commerceContext)
        {
            var propertiesModel = commerceContext.GetObject<PropertiesModel>();
            var discount = commerceContext.GetPolicy<KnownCartAdjustmentTypesPolicy>().Discount;

            var adjustment = new CartLineLevelAwardedAdjustment()
            {
                Name = (propertiesModel?.GetPropertyValue("PromotionText") as string ?? discount),
                DisplayName = (propertiesModel?.GetPropertyValue("PromotionCartText") as string ?? discount),
                Adjustment = new Money(commerceContext.CurrentCurrency(), amountOff),
                AdjustmentType = discount,
                IsTaxable = false,
                AwardingBlock = awardingBlock,
                LineItemId = lineItemId,
            };

            return adjustment;
        }
    }
}
