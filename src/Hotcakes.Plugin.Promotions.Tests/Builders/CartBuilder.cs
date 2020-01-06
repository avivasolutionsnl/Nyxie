using System;
using System.Linq;
using System.Threading.Tasks;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Commerce.Plugin.Payments;

namespace Hotcakes.Plugin.Promotions.Tests.Builders
{
    public class CartBuilder
    {
        private EntityReference fullfilmentMethod = new EntityReference("001", "Standard");
        private bool hasSplitFulfillment;
        private LineBuilder[] lineBuilders = { new LineBuilder() };
        private EntityReference paymentMethod = new EntityReference("001", "Federated");

        public CartBuilder WithStandardFulfillment()
        {
            fullfilmentMethod = new EntityReference("001", "Standard");
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

        public CartBuilder WithSplitFulfillment()
        {
            hasSplitFulfillment = true;
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
            }, new PaymentComponent
            {
                PaymentMethod = paymentMethod
            });

            cart.Lines = lineBuilders.Select(x => x.Build()).ToList();

            cart.AddPolicies(new CalculateCartPolicy { AlwaysCalculate = true });

            if (hasSplitFulfillment)
                cart.AddComponents(new SplitFulfillmentComponent
                {
                    FulfillmentMethod = new EntityReference("002", "Split")
                });
            else
                cart.AddComponents(new FulfillmentComponent
                {
                    FulfillmentMethod = fullfilmentMethod
                });

            return Task.FromResult(cart);
        }
    }
}
