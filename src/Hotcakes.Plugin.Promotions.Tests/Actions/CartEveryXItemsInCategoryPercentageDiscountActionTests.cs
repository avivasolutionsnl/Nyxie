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
    public class CartEveryXItemsInCategoryPercentageDiscountActionTests
    {
        private readonly EngineFixture fixture;

        public CartEveryXItemsInCategoryPercentageDiscountActionTests(EngineFixture engineFixture,
            ITestOutputHelper testOutputHelper)
        {
            engineFixture.SetOutput(testOutputHelper);
            fixture = engineFixture;
        }

        [Fact]
        public async void Should_benefit_when_category_matches()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new IsCurrentDayConditionBuilder()
                                            .Day(DateTime.Now.Day))
                                        .BenefitBy(new CartEveryXItemsInCategoryPercentageDiscountActionBuilder()
                                                   .PercentageOff(50)
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

            Cart cart = await new CartBuilder()
                              .WithLines(new LineBuilder().IdentifiedBy("001").Quantity(1).InCategory("435345345").Price(40),
                                  new LineBuilder().IdentifiedBy("002").Quantity(1).InCategory("435345345").Price(50))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            CartLineComponent line = resultCart.Lines.Single(x => x.Id == "001");
            AwardedAdjustment adjustment =
                line.Adjustments.Single(x => x.AwardingBlock == nameof(CartEveryXItemsInCategoryPercentageDiscountAction));
            Assert.Equal(-20, adjustment.Adjustment.Amount);

            // Subtotal = 70, Tax is 10% = 7, Fulfillment fee = 5
            Assert.Equal(82, resultCart.Totals.GrandTotal.Amount);
        }

        [Fact]
        public async void Should_not_benefit_when_category_does_not_match()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new IsCurrentDayConditionBuilder()
                                            .Day(DateTime.Now.Day))
                                        .BenefitBy(new CartEveryXItemsInCategoryPercentageDiscountActionBuilder()
                                                   .PercentageOff(50)
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

            Cart cart = await new CartBuilder()
                              .WithLines(new LineBuilder().IdentifiedBy("001").Quantity(1).InCategory("34345454").Price(40),
                                  new LineBuilder().IdentifiedBy("002").Quantity(1).InCategory("454545454").Price(50))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            CartLineComponent line = resultCart.Lines.Single(x => x.Id == "002");

            Assert.DoesNotContain(line.Adjustments,
                x => x.AwardingBlock == nameof(CartEveryXItemsInCategoryPercentageDiscountAction));
        }

        [Theory]
        [InlineData(3, 1)]
        [InlineData(3, 2)]
        public async void Should_not_benefit_when_items_to_purchase_does_not_match(int itemsToPurchase, int quantityInCart)
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new IsCurrentDayConditionBuilder()
                                            .Day(DateTime.Now.Day))
                                        .BenefitBy(new CartEveryXItemsInCategoryPercentageDiscountActionBuilder()
                                                   .PercentageOff(50)
                                                   .ForCategory("Laptops")
                                                   .ItemsToAward(2)
                                                   .ItemsToPurchase(itemsToPurchase)
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

            Cart cart = await new CartBuilder()
                              .WithLines(new LineBuilder()
                                         .IdentifiedBy("001").Quantity(quantityInCart).InCategory("435345345").Price(40))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            CartLineComponent line = resultCart.Lines.Single(x => x.Id == "001");

            Assert.DoesNotContain(line.Adjustments,
                x => x.AwardingBlock == nameof(CartEveryXItemsInCategoryPercentageDiscountAction));
        }

        [Fact]
        public async void Should_benefit_in_descending_order_when_category_matches()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new IsCurrentDayConditionBuilder()
                                            .Day(DateTime.Now.Day))
                                        .BenefitBy(new CartEveryXItemsInCategoryPercentageDiscountActionBuilder()
                                                   .PercentageOff(50)
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

            Cart cart = await new CartBuilder()
                              .WithLines(new LineBuilder().IdentifiedBy("001").Quantity(1).InCategory("435345345").Price(40),
                                  new LineBuilder().IdentifiedBy("002").Quantity(1).InCategory("435345345").Price(50))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            CartLineComponent line = resultCart.Lines.Single(x => x.Id == "002");
            AwardedAdjustment adjustment =
                line.Adjustments.Single(x => x.AwardingBlock == nameof(CartEveryXItemsInCategoryPercentageDiscountAction));
            Assert.Equal(-25, adjustment.Adjustment.Amount);

            // Subtotal = 65, Tax is 10% = 6.5, Fulfillment fee = 5
            Assert.Equal(76.5m, resultCart.Totals.GrandTotal.Amount);
        }

        [Fact]
        public async void Should_benefit_multiple_times_when_within_action_limit()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new IsCurrentDayConditionBuilder()
                                            .Day(DateTime.Now.Day))
                                        .BenefitBy(new CartEveryXItemsInCategoryPercentageDiscountActionBuilder()
                                                   .PercentageOff(50)
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

            Cart cart = await new CartBuilder()
                              .WithLines(new LineBuilder().IdentifiedBy("001").Quantity(1).InCategory("435345345").Price(40),
                                  new LineBuilder().IdentifiedBy("002").Quantity(1).InCategory("435345345").Price(50),
                                  new LineBuilder().IdentifiedBy("003").Quantity(1).InCategory("435345345").Price(40),
                                  new LineBuilder().IdentifiedBy("004").Quantity(1).InCategory("435345345").Price(50))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            CartLineComponent firstLine = resultCart.Lines.Single(x => x.Id == "001");
            AwardedAdjustment adjustment =
                firstLine.Adjustments.Single(x => x.AwardingBlock == nameof(CartEveryXItemsInCategoryPercentageDiscountAction));
            Assert.Equal(-20, adjustment.Adjustment.Amount);

            CartLineComponent secondLine = resultCart.Lines.Single(x => x.Id == "003");
            AwardedAdjustment secondAdjustment =
                secondLine.Adjustments.Single(x => x.AwardingBlock == nameof(CartEveryXItemsInCategoryPercentageDiscountAction));
            Assert.Equal(-20, secondAdjustment.Adjustment.Amount);

            // Subtotal = 140, Tax is 10% = 14, Fulfillment fee = 5
            Assert.Equal(159, resultCart.Totals.GrandTotal.Amount);
        }

        [Fact]
        public async void Should_not_benefit_multiple_times_when_exceeds_action_limit()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new IsCurrentDayConditionBuilder()
                                            .Day(DateTime.Now.Day))
                                        .BenefitBy(new CartEveryXItemsInCategoryPercentageDiscountActionBuilder()
                                                   .PercentageOff(50)
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

            Cart cart = await new CartBuilder()
                              .WithLines(new LineBuilder().IdentifiedBy("001").Quantity(1).InCategory("435345345").Price(40),
                                  new LineBuilder().IdentifiedBy("002").Quantity(1).InCategory("435345345").Price(50),
                                  new LineBuilder().IdentifiedBy("003").Quantity(1).InCategory("435345345").Price(40),
                                  new LineBuilder().IdentifiedBy("004").Quantity(1).InCategory("435345345").Price(50))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            CartLineComponent firstLine = resultCart.Lines.Single(x => x.Id == "001");
            AwardedAdjustment adjustment =
                firstLine.Adjustments.Single(x => x.AwardingBlock == nameof(CartEveryXItemsInCategoryPercentageDiscountAction));
            Assert.Equal(-20, adjustment.Adjustment.Amount);

            CartLineComponent secondLine = resultCart.Lines.Single(x => x.Id == "003");
            Assert.DoesNotContain(secondLine.Adjustments,
                x => x.AwardingBlock == nameof(CartEveryXItemsInCategoryPercentageDiscountAction));

            // Subtotal = 160, Tax is 10% = 16, Fulfillment fee = 5
            Assert.Equal(181, resultCart.Totals.GrandTotal.Amount);
        }
    }
}
