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
        public IBinaryOperator<string, string> Operator { get; set; }

        public IRuleValue<string> SpecificPayment { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            //Get configuration
            var specificPayment = SpecificPayment.Yield(context);
            if (string.IsNullOrEmpty(specificPayment) || Operator == null)
            {
                return false;
            }

            //Get Data
            if (!GetPayment(context, out var payment))
            {
                return false;
            }

            //Validate data against configuration
            return Operator.Evaluate(payment.PaymentMethod.Name, specificPayment);
        }

        private bool GetPayment(IRuleExecutionContext context, out PaymentComponent payment)
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
