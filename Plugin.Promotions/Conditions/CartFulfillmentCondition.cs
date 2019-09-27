using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Framework.Rules;

namespace Promethium.Plugin.Promotions.Conditions
{
    /// <summary>
    /// A SiteCore Commerce condition for the qualification
    /// "Cart has [operator] [specific fulfillment]"
    /// </summary>
    [EntityIdentifier("Promethium_" + nameof(CartFulfillmentCondition))]
    public class CartFulfillmentCondition : ICondition, IFulfillmentCondition
    {
        public IBinaryOperator<string, string> Operator { get; set; }

        public IRuleValue<string> SpecificFullfillment { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            //Get configuration
            var specificFullfillment = SpecificFullfillment.Yield(context);
            if (string.IsNullOrEmpty(specificFullfillment) || Operator == null)
            {
                return false;
            }

            //Get Data
            if (!GetFulfillment(context, out var fulfillment))
            {
                return false;
            }

            //Validate data against configuration
            return Operator.Evaluate(fulfillment.FulfillmentMethod.Name, specificFullfillment);
        }

        private bool GetFulfillment(IRuleExecutionContext context, out FulfillmentComponent fulfillment)
        {
            fulfillment = null;

            var cart = context.Fact<CommerceContext>()?.GetObject<Cart>();
            if (cart == null && !cart.Lines.Any<CartLineComponent>() || !cart.HasComponent<FulfillmentComponent>())
            {
                return false;
            }

            fulfillment = cart.GetComponent<FulfillmentComponent>();
            return true;
        }
    }
}
