using Promethium.Plugin.Promotions.Classes;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Payments;
using Sitecore.Framework.Rules;
using System.Linq;

namespace Promethium.Plugin.Promotions.Conditions
{
    /// <summary>
    /// A Sitecore Commerce condition for the qualification
    /// "Cart has [operator] [specific payment]"
    /// </summary>
    [EntityIdentifier("Pm_" + nameof(CartPaymentCondition))]
    public class CartPaymentCondition : ICondition
    {
        public IRuleValue<string> Pm_BasicStringCompare { get; set; }

        public IRuleValue<string> Pm_SpecificPayment { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            //Get configuration
            var specificPayment = Pm_SpecificPayment.Yield(context);
            var basicStringCompare = Pm_BasicStringCompare.Yield(context);
            if (string.IsNullOrEmpty(specificPayment) || string.IsNullOrEmpty(basicStringCompare))
            {
                return false;
            }

            //Get Data
            var cart = context.Fact<CommerceContext>()?.GetObject<Cart>();
            if (cart == null || !cart.Lines.Any() || !cart.HasComponent<PaymentComponent>())
            {
                return false;
            }

            var payment = cart.GetComponent<PaymentComponent>();
            if (payment == null)
            {
                return false;
            }

            //Validate data against configuration
            var selectedPayment = payment.PaymentMethod.Name;
            return BasicStringComparer.Evaluate(basicStringCompare, selectedPayment, specificPayment);
        }
    }
}
