using System.Linq;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Payments;
using Sitecore.Framework.Rules;

namespace Nyxie.Plugin.Promotions.Conditions
{
    /// <summary>
    ///     A Sitecore Commerce condition for the qualification
    ///     "Cart has [operator] [specific payment]"
    /// </summary>
    [EntityIdentifier("Ny_" + nameof(CartPaymentCondition))]
    public class CartPaymentCondition : ICondition
    {
        public IRuleValue<string> Ny_BasicStringCompare { get; set; }

        public IRuleValue<string> Ny_SpecificPayment { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            //Get configuration
            string specificPayment = Ny_SpecificPayment.Yield(context);
            string basicStringCompare = Ny_BasicStringCompare.Yield(context);
            if (string.IsNullOrEmpty(specificPayment) || string.IsNullOrEmpty(basicStringCompare))
                return false;

            //Get Data
            var cart = context.Fact<CommerceContext>()?.GetObject<Cart>();
            if (cart == null || !cart.Lines.Any() || !cart.HasComponent<PaymentComponent>())
                return false;

            var payment = cart.GetComponent<PaymentComponent>();
            if (payment == null)
                return false;

            //Validate data against configuration
            string selectedPayment = payment.PaymentMethod.Name;
            return BasicStringComparer.Evaluate(basicStringCompare, selectedPayment, specificPayment);
        }
    }
}
