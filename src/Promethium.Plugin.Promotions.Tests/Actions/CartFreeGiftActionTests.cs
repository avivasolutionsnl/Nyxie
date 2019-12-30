using System;
using System.Linq;

using Promethium.Plugin.Promotions.Actions;
using Promethium.Plugin.Promotions.Tests.Builders;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Promotions;

using Xunit;
using Xunit.Abstractions;

namespace Promethium.Plugin.Promotions.Tests.Actions
{
    [Collection("Engine collection")]
    public class CartFreeGiftActionTests
    {
        private readonly EngineFixture fixture;

        public CartFreeGiftActionTests(EngineFixture engineFixture, ITestOutputHelper testOutputHelper)
        {
            engineFixture.SetOutput(testOutputHelper);
            fixture = engineFixture;
        }

        [Fact]
        public async void Should_benefit()
        {
            fixture.Factory.ClearAllEntities();

            var client = fixture.Factory.CreateClient();

            var promotion = await new PromotionBuilder()
                                  .QualifiedBy(new IsCurrentDayConditionBuilder()
                                      .Day(DateTime.Now.Day))
                                  .BenefitBy(new CartFreeGiftActionBuilder()
                                      .Quantity(1)
                                      .Gift("MyCatalog|999|"))
                                  .Build(fixture.Factory);

            var catalog = new Catalog
            {
                Id = "Entity-Catalog-MyCatalog", 
                FriendlyId = "MyCatalog",
                Name = "MyCatalog"
            };

            var sellableItem = new SellableItemBuilder()
                               .IdentifiedBy("001")
                               .Priced(40)
                               .Catalog("MyCatalog")
                               .Build();


            var gift = new SellableItemBuilder()
                       .IdentifiedBy("999")
                       .Catalog("MyCatalog")
                       .Named("Free Gift")
                       .Priced(99)
                       .Build();

            var cart = await new CartBuilder()
                             .WithLines(new LineBuilder().IdentifiedBy("001").WithProductId("MyCatalog|001|").Quantity(1).Price(40))
                             .Build();

            fixture.Factory.AddEntities(promotion, gift, sellableItem, catalog, cart);
            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            
            var resultCart = await client.GetJsonAsync<Cart>("api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");
            
            var giftLine = resultCart.Lines.Single(x => x.ItemId == "MyCatalog|999|");

            var adjustment = giftLine.Adjustments.Single(x => x.AwardingBlock == nameof(CartFreeGiftAction));
            Assert.Equal(-99, adjustment.Adjustment.Amount);

            // Subtotal = 40, Tax is 10% = 4, Fulfillment fee = 5
            Assert.Equal(49, resultCart.Totals.GrandTotal.Amount);
        }
    }
}
