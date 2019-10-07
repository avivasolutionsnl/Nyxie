using Promethium.Plugin.Promotions.Classes;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Pricing;
using System;
using System.Collections.Generic;

namespace Promethium.Plugin.Promotions.Extensions
{
    internal static class ActionExtensions
    {
        internal static void ApplyAction(this IEnumerable<CartLineComponent> categoryLines,
            CommerceContext commerceContext, decimal initialDiscount, string applyAwardTo, decimal awardLimit,
            string awardingBlock, Func<decimal, decimal, decimal> discountFunc)
        {
            categoryLines = ActionProductOrdener.Order(categoryLines, applyAwardTo);

            var counter = 0;
            foreach (var line in categoryLines)
            {
                var discount = discountFunc(line.UnitListPrice.Amount, initialDiscount);
                discount = discount.ShouldRoundPriceCalc(commerceContext);

                for (var i = 0; i < line.Quantity; i++)
                {
                    if (counter == awardLimit)
                    {
                        break;
                    }

                    line.Adjustments.AddLineLevelAwardedAdjustment(commerceContext, discount * -1, awardingBlock, line.ItemId);
                    line.Totals.SubTotal.Amount = line.Totals.SubTotal.Amount - discount;

                    line.GetComponent<MessagesComponent>().AddPromotionApplied(commerceContext, awardingBlock);

                    counter++;
                }
            }
        }

        internal static decimal CalculateAmountDiscount(decimal productPrice, decimal amountOff)
        {
            return amountOff > productPrice ? productPrice : amountOff;
        }

        internal static decimal CalculatePercentageDiscount(decimal productPrice, decimal percentage)
        {
            return productPrice * (percentage / 100);
        }

        internal static decimal ShouldRoundPriceCalc(this decimal input, CommerceContext context)
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

        internal static void AddCartLevelAwardedAdjustment(this IList<AwardedAdjustment> adjustments, CommerceContext context, decimal amountOff, string awardingBlock)
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
                AwardingBlock = awardingBlock,
            };

            adjustments.Add(adjustment);
        }

        internal static void AddLineLevelAwardedAdjustment(this IList<AwardedAdjustment> adjustments, CommerceContext context, decimal amountOff, string awardingBlock, string lineItemId)
        {
            var propertiesModel = context.GetObject<PropertiesModel>();
            var discount = context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Discount;

            var adjustment = new CartLineLevelAwardedAdjustment()
            {
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

        internal static void AddPromotionApplied(this MessagesComponent messageComponent, CommerceContext context, string awardingBlock)
        {
            var propertiesModel = context.GetObject<PropertiesModel>();
            var promotionName = propertiesModel?.GetPropertyValue("PromotionId") ?? awardingBlock;
            messageComponent.AddMessage(context.GetPolicy<KnownMessageCodePolicy>().Promotions, $"PromotionApplied: {promotionName}");
        }
    }
}
