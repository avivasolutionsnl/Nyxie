using System;
using System.Linq;
using System.Threading.Tasks;

using Promethium.Plugin.Promotions.Components;

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

    public class LineBuilder
    {
        private string itemId = "001";
        private decimal quantity = 1;
        private decimal price = 33;
        private string categorySitecoreId = null;

        public LineBuilder Quantity(decimal quantity)
        {
            this.quantity = quantity;
            return this;
        }

        public LineBuilder Price(decimal price)
        {
            this.price = price;
            return this;
        }

        public LineBuilder InCategory(string categorySitecoreId)
        {
            this.categorySitecoreId = categorySitecoreId;
            return this;
        }
        
        public CartLineComponent Build()
        {
            var line = new CartLineComponent
            {
                Quantity = quantity,
                ItemId = itemId,
                Policies =
                {
                    new PurchaseOptionMoneyPolicy
                    {
                        SellPrice = new Money(price),
                    }
                }
            };

            if (categorySitecoreId != null)
            {
                var categoryComponent = line.GetComponent<CategoryComponent>();
                categoryComponent.ParentCategoryList.Add(categorySitecoreId);
            }

            return line;
        }
    }
}
