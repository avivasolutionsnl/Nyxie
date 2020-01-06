using System;
using System.Linq;
using System.Net.Http;

using Hotcakes.Plugin.Promotions.Actions;
using Hotcakes.Plugin.Promotions.Tests.Builders;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Commerce.Plugin.Promotions;

using Xunit;
using Xunit.Abstractions;

namespace Hotcakes.Plugin.Promotions.Tests.Actions
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

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
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

            SellableItem sellableItem = new SellableItemBuilder()
                                        .IdentifiedBy("001")
                                        .Priced(40)
                                        .Catalog("MyCatalog")
                                        .Build();

            SellableItem gift = new SellableItemBuilder()
                                .IdentifiedBy("999")
                                .Catalog("MyCatalog")
                                .Named("Free Gift")
                                .Priced(99)
                                .Build();

            Cart cart = await new CartBuilder()
                              .WithLines(new LineBuilder()
                                         .IdentifiedBy("001").WithProductId("MyCatalog|001|").Quantity(1).Price(40))
                              .Build();

            fixture.Factory.AddEntities(promotion, gift, sellableItem, catalog, cart);
            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            CartLineComponent giftLine = resultCart.Lines.Single(x => x.ItemId == "MyCatalog|999|");

            AwardedAdjustment adjustment = giftLine.Adjustments.Single(x => x.AwardingBlock == nameof(CartFreeGiftAction));
            Assert.Equal(-99, adjustment.Adjustment.Amount);

            // Subtotal = 40, Tax is 10% = 4, Fulfillment fee = 5
            Assert.Equal(49, resultCart.Totals.GrandTotal.Amount);
        }
    }
}
