using Promethium.Plugin.Promotions.Classes;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Pricing;
using System;
using System.Collections.Generic;

namespace Promethium.Plugin.Promotions.Factory
{
    internal class ActionFactory
    {
        private readonly CommerceContext _commerceContext;

        internal ActionFactory(CommerceContext commerceContext)
        {
            _commerceContext = commerceContext;
        }

        internal void ApplyAction(IEnumerable<CartLineComponent> categoryLines,
            decimal discountValue, string applyActionTo, int actionLimit,
            string awardingBlock, Func<decimal, decimal, decimal> calculateDiscount)
        {
            categoryLines = ActionProductOrdener.Order(categoryLines, applyActionTo);

            var counter = 0;
            foreach (var line in categoryLines)
            {
                var discount = calculateDiscount(line.UnitListPrice.Amount, discountValue);
                discount = ShouldRoundPriceCalc(discount);

                for (var i = 0;i < line.Quantity;i++)
                {
                    if (counter == actionLimit)
                    {
                        return;
                    }

                    line.Adjustments.Add(CreateLineLevelAwardedAdjustment(discount * -1, awardingBlock, line.ItemId));
                    line.Totals.SubTotal.Amount -= discount;

                    AddPromotionApplied(line.GetComponent<MessagesComponent>(), awardingBlock);

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

        internal decimal ShouldRoundPriceCalc(decimal input)
        {
            if (_commerceContext.GetPolicy<GlobalPricingPolicy>().ShouldRoundPriceCalc)
            {
                input = Math.Round(input,
                    _commerceContext.GetPolicy<GlobalPricingPolicy>().RoundDigits,
                    _commerceContext.GetPolicy<GlobalPricingPolicy>().MidPointRoundUp
                        ? MidpointRounding.AwayFromZero
                        : MidpointRounding.ToEven);
            }

            return input;
        }

        internal CartLevelAwardedAdjustment CreateCartLevelAwardedAdjustment(decimal amountOff, string awardingBlock)
        {
            var propertiesModel = _commerceContext.GetObject<PropertiesModel>();
            var discount = _commerceContext.GetPolicy<KnownCartAdjustmentTypesPolicy>().Discount;

            var adjustment = new CartLevelAwardedAdjustment {
                Name = propertiesModel?.GetPropertyValue("PromotionText") as string ?? discount,
                DisplayName = propertiesModel?.GetPropertyValue("PromotionCartText") as string ?? discount,
                Adjustment = new Money(_commerceContext.CurrentCurrency(), amountOff),
                AdjustmentType = discount,
                IsTaxable = false,
                AwardingBlock = awardingBlock,
            };

            return adjustment;
        }

        internal CartLineLevelAwardedAdjustment CreateLineLevelAwardedAdjustment(decimal amountOff, string awardingBlock, string lineItemId)
        {
            var propertiesModel = _commerceContext.GetObject<PropertiesModel>();
            var discount = _commerceContext.GetPolicy<KnownCartAdjustmentTypesPolicy>().Discount;

            var adjustment = new CartLineLevelAwardedAdjustment() {
                Name = (propertiesModel?.GetPropertyValue("PromotionText") as string ?? discount),
                DisplayName = (propertiesModel?.GetPropertyValue("PromotionCartText") as string ?? discount),
                Adjustment = new Money(_commerceContext.CurrentCurrency(), amountOff),
                AdjustmentType = discount,
                IsTaxable = false,
                AwardingBlock = awardingBlock,
                LineItemId = lineItemId,
            };

            return adjustment;
        }

        internal void AddPromotionApplied(MessagesComponent messageComponent, string awardingBlock)
        {
            var propertiesModel = _commerceContext.GetObject<PropertiesModel>();
            var promotionName = propertiesModel?.GetPropertyValue("PromotionId") ?? awardingBlock;
            messageComponent.AddMessage(_commerceContext.GetPolicy<KnownMessageCodePolicy>().Promotions, $"PromotionApplied: {promotionName}");
        }
    }
}
