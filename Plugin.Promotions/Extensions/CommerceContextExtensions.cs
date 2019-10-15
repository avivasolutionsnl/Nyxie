using Promethium.Plugin.Promotions.Classes;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Pricing;
using System;
using System.Collections.Generic;

namespace Promethium.Plugin.Promotions.Extensions
{
    internal static class CommerceContextExtensions
    {
        internal static void ApplyAction(this CommerceContext commerceContext, IEnumerable<CartLineComponent> categoryLines,
            decimal discountValue, string applyActionTo, int actionLimit,
            string awardingBlock, Func<decimal, decimal, decimal> calculateDiscount)
        {
            categoryLines = ActionProductOrdener.Order(categoryLines, applyActionTo);

            var counter = 0;
            foreach (var line in categoryLines)
            {
                var discount = calculateDiscount(line.UnitListPrice.Amount, discountValue);
                discount = commerceContext.ShouldRoundPriceCalc(discount);

                for (var i = 0;i < line.Quantity;i++)
                {
                    if (counter == actionLimit)
                    {
                        return;
                    }

                    line.Adjustments.AddLineLevelAwardedAdjustment(commerceContext, discount * -1, awardingBlock, line.ItemId);
                    line.Totals.SubTotal.Amount -= discount;

                    commerceContext.AddPromotionApplied(line.GetComponent<MessagesComponent>(), awardingBlock);

                    counter++;
                }
            }
        }

        internal static decimal CalculatePriceDiscount(decimal productPrice, decimal amountOff)
        {
            return amountOff > productPrice ? productPrice : amountOff;
        }

        internal static decimal CalculatePercentageDiscount(decimal productPrice, decimal percentage)
        {
            return productPrice * (percentage / 100);
        }

        internal static decimal ShouldRoundPriceCalc(this CommerceContext context, decimal input)
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

        internal static void AddPromotionApplied(this CommerceContext context, MessagesComponent messageComponent, string awardingBlock)
        {
            var propertiesModel = context.GetObject<PropertiesModel>();
            var promotionName = propertiesModel?.GetPropertyValue("PromotionId") ?? awardingBlock;
            messageComponent.AddMessage(context.GetPolicy<KnownMessageCodePolicy>().Promotions, $"PromotionApplied: {promotionName}");
        }
    }
}