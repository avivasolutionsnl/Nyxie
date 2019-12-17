using System;
using System.Threading.Tasks;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Commerce.Plugin.Payments;
using Sitecore.Commerce.Plugin.Pricing;

namespace Promethium.Plugin.Promotions.Tests.Builders
{
    public class CartBuilder
    {
        private EntityReference fullfilmentMethod = new EntityReference("001", "Standard");
        private EntityReference paymentMethod = new EntityReference("001", "Federated");

        public CartBuilder WithFulfillment(EntityReference fullfilmentMethod)
        {
            this.fullfilmentMethod = fullfilmentMethod;
            return this;
        }

        public CartBuilder WithPaymentMethod(EntityReference paymentMethod)
        {
            this.paymentMethod = paymentMethod;
            return this;
        }

        public Task<Cart> Build()
        {
            var cart = new Cart
            {
                Id = "Cart01",
                DateUpdated = DateTimeOffset.UtcNow
            };
            cart.AddComponents(new ContactComponent
            {
                Language = "en"
            }, new FulfillmentComponent
            {
                FulfillmentMethod = fullfilmentMethod
            }, new PaymentComponent
            {
                PaymentMethod = paymentMethod
            });

            cart.Lines.Add(new CartLineComponent
            {
                Quantity = 1,
                ItemId = "001",
                Policies = { new PurchaseOptionMoneyPolicy
                {
                    SellPrice = new Money(33),
                }}
            });

            cart.AddPolicies(new CalculateCartPolicy { AlwaysCalculate = true });

            return Task.FromResult(cart);
        }
    }
}
