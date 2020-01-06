using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Availability;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Pricing;

namespace Hotcakes.Plugin.Promotions.Tests.Builders
{
    public class SellableItemBuilder
    {
        private string catalog = "My Catalog";
        private string id = "001";
        private string name = "My product";
        private decimal price = 99;

        public SellableItemBuilder IdentifiedBy(string id)
        {
            this.id = id;
            return this;
        }

        public SellableItemBuilder Named(string name)
        {
            this.name = name;
            return this;
        }

        public SellableItemBuilder Priced(decimal price)
        {
            this.price = price;
            return this;
        }

        public SellableItemBuilder Catalog(string catalog)
        {
            this.catalog = catalog;
            return this;
        }

        public SellableItem Build()
        {
            var sellableItem = new SellableItem
            {
                Id = "Entity-SellableItem-" + id,
                FriendlyId = id,
                ProductId = id,
                Name = name,
                DisplayName = name,
                ListPrice = new Money(price),
                ParentCategoryList = "001",
                CatalogToEntityList = "Entity-Catalog-" + catalog
            };

            sellableItem.AddComponents(new CatalogsComponent
            {
                ChildComponents = new[]
                {
                    new CatalogComponent
                    {
                        Name = catalog
                    }
                }
            });

            sellableItem.AddPolicies(new ListPricingPolicy(new[] { new Money(price) }), new AvailabilityAlwaysPolicy());

            return sellableItem;
        }
    }
}
