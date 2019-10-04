using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Framework.Rules;
using System.Linq;

namespace Promethium.Plugin.Promotions.Actions
{
    /// <summary>
    /// A SiteCore Commerce condition for the qualification
    /// "Get [specific amount] off the shipping cost"
    /// </summary>
    [EntityIdentifier("Promethium_" + nameof(CardAmountOffShippingAction))]
    public class CardAmountOffShippingAction : ICartAction
    {
        public IRuleValue<decimal> Promethium_SpecificAmount { get; set; }

        public void Execute(IRuleExecutionContext context)
        {
            var commerceContext = context.Fact<CommerceContext>();
            var cart = commerceContext?.GetObject<Cart>();
            if (cart == null || !cart.Lines.Any() || !cart.HasComponent<FulfillmentComponent>())
            {
                return;
            }

            var fulfillmentFee = GetFulfillmentFee(cart);
            if (fulfillmentFee == 0)
            {
                return;
            }

            var amountOff = Promethium_SpecificAmount.Yield(context);

            //If the amount off is higher then the total fulfillment fee then lower the amount off
            if (amountOff > fulfillmentFee)
            {
                amountOff = fulfillmentFee;
            }

            amountOff = amountOff.ShouldRoundPriceCalc(commerceContext);

            cart.Adjustments.AddCartLevelAwardedAdjustment(commerceContext, amountOff * -1, nameof(CardAmountOffShippingAction));

            cart.GetComponent<MessagesComponent>().AddPromotionApplied(commerceContext, nameof(CardAmountOffShippingAction));
        }

        private static decimal GetFulfillmentFee(Cart cart)
        {
            if (cart.HasComponent<SplitFulfillmentComponent>())
            {
                return cart.Lines
                    .Select(line => line.Adjustments.FirstOrDefault(a => a.Name.EqualsOrdinalIgnoreCase("FulfillmentFee")))
                    .TakeWhile(lineFulfillment => lineFulfillment != null)
                    .Sum(lineFulfillment => lineFulfillment.Adjustment.Amount);
            }

            var awardedAdjustment = cart.Adjustments.FirstOrDefault(a => a.Name.EqualsOrdinalIgnoreCase("FulfillmentFee"));
            return awardedAdjustment?.Adjustment.Amount ?? 0;
        }
    }
}