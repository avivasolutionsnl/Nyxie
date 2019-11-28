using Promethium.Plugin.Promotions.Classes;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Framework.Rules;
using System.Linq;

namespace Promethium.Plugin.Promotions.Conditions
{
    /// <summary>
    /// A Sitecore Commerce condition for the qualification
    /// "Cart has [operator] [specific fulfillment]"
    /// </summary>
    [EntityIdentifier("Pm_" + nameof(CartFulfillmentCondition))]
    public class CartFulfillmentCondition : IFulfillmentCondition
    {
        public IRuleValue<string> Pm_BasicStringCompare { get; set; }

        public IRuleValue<string> Pm_SpecificFulfillment { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            //Get configuration
            var specificFulfillment = Pm_SpecificFulfillment.Yield(context);
            var basicStringCompare = Pm_BasicStringCompare.Yield(context);
            if (string.IsNullOrEmpty(specificFulfillment) || string.IsNullOrEmpty(basicStringCompare))
            {
                return false;
            }

            //Get Data
            var cart = context.Fact<CommerceContext>()?.GetObject<Cart>();
            if (cart == null || !cart.Lines.Any() || !cart.HasComponent<FulfillmentComponent>())
            {
                return false;
            }

            var fulfillment = cart.GetComponent<FulfillmentComponent>();
            if (fulfillment == null)
            {
                return false;
            }

            //Validate data against configuration
            var selectedFulfillment = fulfillment.FulfillmentMethod.Name;
            return BasicStringComparer.Evaluate(basicStringCompare, selectedFulfillment, specificFulfillment);
        }
    }
}
