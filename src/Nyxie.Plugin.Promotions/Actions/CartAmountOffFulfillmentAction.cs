using System.Linq;

using Nyxie.Plugin.Promotions.Extensions;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Rules;

namespace Nyxie.Plugin.Promotions.Actions
{
    /// <summary>
    ///     A Sitecore Commerce action for the benefit
    ///     "Get [specific amount] off the shipping cost"
    /// </summary>
    [EntityIdentifier("Hc_" + nameof(CartAmountOffFulfillmentAction))]
    public class CartAmountOffFulfillmentAction : ICartAction
    {
        public IRuleValue<decimal> Hc_SpecificAmount { get; set; }

        public void Execute(IRuleExecutionContext context)
        {
            //Get configuration
            var commerceContext = context.Fact<CommerceContext>();
            var cart = commerceContext?.GetObject<Cart>();
            if (cart == null || !cart.Lines.Any() || !cart.HasComponent<FulfillmentComponent>())
                return;

            decimal amountOff = Hc_SpecificAmount.Yield(context);
            if (amountOff == 0)
                return;

            //Get data
            decimal fulfillmentFee = GetFulfillmentFee(cart);
            if (fulfillmentFee == 0)
                return;

            if (amountOff > fulfillmentFee)
                amountOff = fulfillmentFee;

            //Apply action
            amountOff = new MoneyEx(commerceContext, amountOff).Round().Value.Amount;

            CartLevelAwardedAdjustment adjustment = AwardedAdjustmentFactory.CreateCartLevelAwardedAdjustment(amountOff * -1,
                nameof(CartAmountOffFulfillmentAction), commerceContext);
            cart.Adjustments.Add(adjustment);

            cart.GetComponent<MessagesComponent>()
                .AddPromotionApplied(commerceContext, nameof(CartAmountOffFulfillmentAction));
        }

        private static decimal GetFulfillmentFee(Cart cart)
        {
            if (cart.HasComponent<SplitFulfillmentComponent>())
                return cart.Lines
                           .Select(line => line.Adjustments.FirstOrDefault(a => a.Name.EqualsOrdinalIgnoreCase("FulfillmentFee")))
                           .TakeWhile(lineFulfillment => lineFulfillment != null)
                           .Sum(lineFulfillment => lineFulfillment.Adjustment.Amount);

            AwardedAdjustment awardedAdjustment =
                cart.Adjustments.FirstOrDefault(a => a.Name.EqualsOrdinalIgnoreCase("FulfillmentFee"));
            return awardedAdjustment?.Adjustment.Amount ?? 0;
        }
    }
}
