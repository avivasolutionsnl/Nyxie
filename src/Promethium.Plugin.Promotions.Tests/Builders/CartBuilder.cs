using System;
using System.Linq;
using System.Threading.Tasks;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Commerce.Plugin.Payments;

namespace Promethium.Plugin.Promotions.Tests.Builders
{
    public class CartBuilder
    {
        private EntityReference fullfilmentMethod = new EntityReference("001", "Standard");
        private EntityReference paymentMethod = new EntityReference("001", "Federated");
        private LineBuilder[] lineBuilders = new[] { new LineBuilder() };

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

        public CartBuilder WithLines(params LineBuilder[] lineBuilders)
        {
            this.lineBuilders = lineBuilders;
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

            cart.Lines = lineBuilders.Select(x => x.Build()).ToList();

            cart.AddPolicies(new CalculateCartPolicy { AlwaysCalculate = true });

            return Task.FromResult(cart);
        }
    }
}
