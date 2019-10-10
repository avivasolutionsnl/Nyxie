using Promethium.Plugin.Promotions.Classes;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Payments;
using Sitecore.Framework.Rules;
using System.Linq;

namespace Promethium.Plugin.Promotions.Conditions
{
    /// <summary>
    /// A SiteCore Commerce condition for the qualification
    /// "Cart has [operator] [specific payment]"
    /// </summary>
    [EntityIdentifier("Promethium_" + nameof(CartPaymentCondition))]
    public class CartPaymentCondition : ICondition
    {
        public IRuleValue<string> Promethium_BasicStringCompare { get; set; }

        public IRuleValue<string> Promethium_SpecificPayment { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            //Get configuration
            var specificPayment = Promethium_SpecificPayment.Yield(context);
            var basicStringCompare = Promethium_BasicStringCompare.Yield(context);
            if (string.IsNullOrEmpty(specificPayment) || string.IsNullOrEmpty(basicStringCompare))
            {
                return false;
            }

            //Get Data
            if (!GetPayment(context, out var payment))
            {
                return false;
            }

            //Validate data against configuration
            var selectedPayment = payment.PaymentMethod.Name;
            return BasicStringComparer.Evaluate(basicStringCompare, selectedPayment, specificPayment);
        }

        private static bool GetPayment(IRuleExecutionContext context, out PaymentComponent payment)
        {
            payment = null;

            var cart = context.Fact<CommerceContext>()?.GetObject<Cart>();
            if (cart == null || !cart.Lines.Any() || !cart.HasComponent<PaymentComponent>())
            {
                return false;
            }

            payment = cart.GetComponent<PaymentComponent>();
            return true;
        }
    }
}
