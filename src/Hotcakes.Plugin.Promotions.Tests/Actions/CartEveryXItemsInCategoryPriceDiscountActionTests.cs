using System;
using System.Linq;

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
    public class CartEveryXItemsInCategoryPriceDiscountActionTests
    {
        private readonly EngineFixture fixture;

        public CartEveryXItemsInCategoryPriceDiscountActionTests(EngineFixture engineFixture, ITestOutputHelper testOutputHelper)
        {
            engineFixture.SetOutput(testOutputHelper);
            fixture = engineFixture;
        }

        [Fact]
        public async void Should_benefit_when_category_matches()
        {
            fixture.Factory.ClearAllEntities();

            var client = fixture.Factory.CreateClient();

            var promotion = await new PromotionBuilder()
                                  .QualifiedBy(new IsCurrentDayConditionBuilder()
                                      .Day(DateTime.Now.Day))
                                  .BenefitBy(new CartEveryXItemsInCategoryPriceDiscountActionBuilder()
                                      .AmountOff(10)
                                      .ForCategory("Laptops")
                                      .ItemsToAward(1)
                                      .ItemsToPurchase(2)
                                      .ApplyActionTo(ApplicationOrder.Ascending)
                                      .ActionLimit(1))
                                  .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            fixture.Factory.AddEntity(new Category
            {
                Id = "Laptops".ToEntityId<Category>(),
                SitecoreId = "435345345"
            });

            var cart = await new CartBuilder()
                             .WithLines(new LineBuilder().IdentifiedBy("001").Quantity(1).InCategory("435345345").Price(40),
                                 new LineBuilder().IdentifiedBy("002").Quantity(1).InCategory("435345345").Price(50))
                             .Build();

            fixture.Factory.AddEntity(cart);

            var resultCart = await client.GetJsonAsync<Cart>("api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            var line = resultCart.Lines.Single(x => x.Id == "001");
            AwardedAdjustment adjustment = line.Adjustments.Single(x => x.AwardingBlock == nameof(CartEveryXItemsInCategoryPriceDiscountAction));
            Assert.Equal(-10, adjustment.Adjustment.Amount);

            // Subtotal = 80, Tax is 10% = 8, Fulfillment fee = 5
            Assert.Equal(93, resultCart.Totals.GrandTotal.Amount);
        }

        [Fact]
        public async void Should_not_benefit_when_category_does_not_match()
        {
            fixture.Factory.ClearAllEntities();

            var client = fixture.Factory.CreateClient();

            var promotion = await new PromotionBuilder()
                                  .QualifiedBy(new IsCurrentDayConditionBuilder()
                                      .Day(DateTime.Now.Day))
                                  .BenefitBy(new CartEveryXItemsInCategoryPriceDiscountActionBuilder()
                                      .AmountOff(10)
                                      .ForCategory("Laptops")
                                      .ItemsToAward(1)
                                      .ItemsToPurchase(2)
                                      .ApplyActionTo(ApplicationOrder.Ascending)
                                      .ActionLimit(1))
                                  .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            fixture.Factory.AddEntity(new Category
            {
                Id = "Laptops".ToEntityId<Category>(),
                SitecoreId = "435345345"
            });

            var cart = await new CartBuilder()
                             .WithLines(new LineBuilder().IdentifiedBy("001").Quantity(1).InCategory("34345454").Price(40),
                                 new LineBuilder().IdentifiedBy("002").Quantity(1).InCategory("454545454").Price(50))
                             .Build();

            fixture.Factory.AddEntity(cart);

            var resultCart = await client.GetJsonAsync<Cart>("api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            var line = resultCart.Lines.Single(x => x.Id == "001");
            
            Assert.DoesNotContain(line.Adjustments, x => x.AwardingBlock == nameof(CartEveryXItemsInCategoryPriceDiscountAction));
        }

        [Fact]
        public async void Should_benefit_in_descending_order_when_category_matches()
        {
            fixture.Factory.ClearAllEntities();

            var client = fixture.Factory.CreateClient();

            var promotion = await new PromotionBuilder()
                                  .QualifiedBy(new IsCurrentDayConditionBuilder()
                                      .Day(DateTime.Now.Day))
                                  .BenefitBy(new CartEveryXItemsInCategoryPriceDiscountActionBuilder()
                                      .AmountOff(10)
                                      .ForCategory("Laptops")
                                      .ItemsToAward(1)
                                      .ItemsToPurchase(2)
                                      .ApplyActionTo(ApplicationOrder.Descending)
                                      .ActionLimit(1))
                                  .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            fixture.Factory.AddEntity(new Category
            {
                Id = "Laptops".ToEntityId<Category>(),
                SitecoreId = "435345345"
            });

            var cart = await new CartBuilder()
                             .WithLines(new LineBuilder().IdentifiedBy("001").Quantity(1).InCategory("435345345").Price(40),
                                 new LineBuilder().IdentifiedBy("002").Quantity(1).InCategory("435345345").Price(50))
                             .Build();

            fixture.Factory.AddEntity(cart);

            var resultCart = await client.GetJsonAsync<Cart>("api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            var line = resultCart.Lines.Single(x => x.Id == "002");
            AwardedAdjustment adjustment = line.Adjustments.Single(x => x.AwardingBlock == nameof(CartEveryXItemsInCategoryPriceDiscountAction));
            Assert.Equal(-10, adjustment.Adjustment.Amount);

            // Subtotal = 80, Tax is 10% = 8, Fulfillment fee = 5
            Assert.Equal(93, resultCart.Totals.GrandTotal.Amount);
        }
        
        [Fact]
        public async void Should_benefit_multiple_times_when_within_action_limit()
        {
            fixture.Factory.ClearAllEntities();

            var client = fixture.Factory.CreateClient();

            var promotion = await new PromotionBuilder()
                                  .QualifiedBy(new IsCurrentDayConditionBuilder()
                                      .Day(DateTime.Now.Day))
                                  .BenefitBy(new CartEveryXItemsInCategoryPriceDiscountActionBuilder()
                                      .AmountOff(10)
                                      .ForCategory("Laptops")
                                      .ItemsToAward(1)
                                      .ItemsToPurchase(2)
                                      .ApplyActionTo(ApplicationOrder.Ascending)
                                      .ActionLimit(2))
                                  .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            fixture.Factory.AddEntity(new Category
            {
                Id = "Laptops".ToEntityId<Category>(),
                SitecoreId = "435345345"
            });

            var cart = await new CartBuilder()
                             .WithLines(new LineBuilder().IdentifiedBy("001").Quantity(1).InCategory("435345345").Price(40),
                                 new LineBuilder().IdentifiedBy("002").Quantity(1).InCategory("435345345").Price(50),
                                 new LineBuilder().IdentifiedBy("003").Quantity(1).InCategory("435345345").Price(40),
                                 new LineBuilder().IdentifiedBy("004").Quantity(1).InCategory("435345345").Price(50))
                             .Build();

            fixture.Factory.AddEntity(cart);

            var resultCart = await client.GetJsonAsync<Cart>("api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            var firstLine = resultCart.Lines.Single(x => x.Id == "001");
            AwardedAdjustment adjustment = firstLine.Adjustments.Single(x => x.AwardingBlock == nameof(CartEveryXItemsInCategoryPriceDiscountAction));
            Assert.Equal(-10, adjustment.Adjustment.Amount);

            var secondLine = resultCart.Lines.Single(x => x.Id == "003");
            AwardedAdjustment secondAdjustment = secondLine.Adjustments.Single(x => x.AwardingBlock == nameof(CartEveryXItemsInCategoryPriceDiscountAction));
            Assert.Equal(-10, secondAdjustment.Adjustment.Amount);

            // Subtotal = 160, Tax is 10% = 16, Fulfillment fee = 5
            Assert.Equal(181, resultCart.Totals.GrandTotal.Amount);
        }

        [Fact]
        public async void Should_not_benefit_multiple_times_when_exceeds_action_limit()
        {
            fixture.Factory.ClearAllEntities();

            var client = fixture.Factory.CreateClient();

            var promotion = await new PromotionBuilder()
                                  .QualifiedBy(new IsCurrentDayConditionBuilder()
                                      .Day(DateTime.Now.Day))
                                  .BenefitBy(new CartEveryXItemsInCategoryPriceDiscountActionBuilder()
                                      .AmountOff(10)
                                      .ForCategory("Laptops")
                                      .ItemsToAward(1)
                                      .ItemsToPurchase(2)
                                      .ApplyActionTo(ApplicationOrder.Ascending)
                                      .ActionLimit(1))
                                  .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            fixture.Factory.AddEntity(new Category
            {
                Id = "Laptops".ToEntityId<Category>(),
                SitecoreId = "435345345"
            });

            var cart = await new CartBuilder()
                             .WithLines(new LineBuilder().IdentifiedBy("001").Quantity(1).InCategory("435345345").Price(40),
                                 new LineBuilder().IdentifiedBy("002").Quantity(1).InCategory("435345345").Price(50),
                                 new LineBuilder().IdentifiedBy("003").Quantity(1).InCategory("435345345").Price(40),
                                 new LineBuilder().IdentifiedBy("004").Quantity(1).InCategory("435345345").Price(50))
                             .Build();

            fixture.Factory.AddEntity(cart);

            var resultCart = await client.GetJsonAsync<Cart>("api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            var firstLine = resultCart.Lines.Single(x => x.Id == "001");
            AwardedAdjustment adjustment = firstLine.Adjustments.Single(x => x.AwardingBlock == nameof(CartEveryXItemsInCategoryPriceDiscountAction));
            Assert.Equal(-10, adjustment.Adjustment.Amount);

            var secondLine = resultCart.Lines.Single(x => x.Id == "003");
            Assert.DoesNotContain(secondLine.Adjustments, x => x.AwardingBlock == nameof(CartEveryXItemsInCategoryPriceDiscountAction));

            // Subtotal = 170, Tax is 10% = 17, Fulfillment fee = 5
            Assert.Equal(192, resultCart.Totals.GrandTotal.Amount);
        }
    }
}
