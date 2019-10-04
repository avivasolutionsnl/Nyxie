using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Pricing;
using System;
using System.Collections.Generic;

namespace Promethium.Plugin.Promotions.Extensions
{
    public static class ActionExtensions
    {
        public static decimal ShouldRoundPriceCalc(this decimal input, CommerceContext context)
        {
            if (context.GetPolicy<GlobalPricingPolicy>().ShouldRoundPriceCalc)
            {
                input = Math.Round(input,
                    context.GetPolicy<GlobalPricingPolicy>().RoundDigits,
                    context.GetPolicy<GlobalPricingPolicy>().MidPointRoundUp
                        ? MidpointRounding.AwayFromZero
                        : MidpointRounding.ToEven);
            }

            return input;
        }

        public static void AddCartLevelAwardedAdjustment(this IList<AwardedAdjustment> adjustments, CommerceContext context, decimal amountOff, string awardingBlock)
        {
            var propertiesModel = context.GetObject<PropertiesModel>();
            var discount = context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Discount;

            var adjustment = new CartLevelAwardedAdjustment
            {
                Name = propertiesModel?.GetPropertyValue("PromotionText") as string ?? discount,
                DisplayName = propertiesModel?.GetPropertyValue("PromotionCartText") as string ?? discount,
                Adjustment = new Money(context.CurrentCurrency(), amountOff),
                AdjustmentType = discount,
                IsTaxable = false,
                AwardingBlock = awardingBlock
            };

            adjustments.Add(adjustment);
        }

        public static void AddPromotionApplied(this MessagesComponent messageComponent, CommerceContext context, string awardingBlock)
        {
            var propertiesModel = context.GetObject<PropertiesModel>();
            var promotionName = propertiesModel?.GetPropertyValue("PromotionId") ?? awardingBlock;
            messageComponent.AddMessage(context.GetPolicy<KnownMessageCodePolicy>().Promotions, $"PromotionApplied: {promotionName}");
        }
    }
}
