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
    public class CartItemsMatchingInCategoryPercentageDiscountActionTests
    {
        private readonly EngineFixture fixture;

        public CartItemsMatchingInCategoryPercentageDiscountActionTests(EngineFixture engineFixture, ITestOutputHelper testOutputHelper)
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
                                  .BenefitBy(new CartItemsMatchingInCategoryPercentageDiscountActionBuilder()
                                      .PercentageOff(50)
                                      .ForCategory("Laptops")
                                      .Operator(Operator.Equal)
                                      .NumberOfProducts(1)
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
                             .WithLines(new LineBuilder().IdentifiedBy("001").Quantity(1).InCategory("435345345").Price(40))
                             .Build();

            fixture.Factory.AddEntity(cart);

            var resultCart = await client.GetJsonAsync<Cart>("api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            var line = resultCart.Lines.Single(x => x.Id == "001");
            AwardedAdjustment adjustment = line.Adjustments.Single(x => x.AwardingBlock == nameof(CartItemsMatchingInCategoryPercentageDiscountAction));
            Assert.Equal(-20, adjustment.Adjustment.Amount);

            // Subtotal = 20, Tax is 10% = 2, Fulfillment fee = 5
            Assert.Equal(27, resultCart.Totals.GrandTotal.Amount);
        }

        [Fact]
        public async void Should_not_benefit_when_category_does_not_match()
        {
            fixture.Factory.ClearAllEntities();

            var client = fixture.Factory.CreateClient();

            var promotion = await new PromotionBuilder()
                                  .QualifiedBy(new IsCurrentDayConditionBuilder()
                                      .Day(DateTime.Now.Day))
                                  .BenefitBy(new CartItemsMatchingInCategoryPercentageDiscountActionBuilder()
                                      .PercentageOff(50)
                                      .ForCategory("Laptops")
                                      .Operator(Operator.Equal)
                                      .NumberOfProducts(1)
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
                             .WithLines(new LineBuilder().IdentifiedBy("001").Quantity(1).InCategory("34345454").Price(40))
                             .Build();

            fixture.Factory.AddEntity(cart);

            var resultCart = await client.GetJsonAsync<Cart>("api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            var line = resultCart.Lines.Single(x => x.Id == "001");
            
            Assert.DoesNotContain(line.Adjustments, x => x.AwardingBlock == nameof(CartItemsMatchingInCategoryPercentageDiscountAction));
        }

        [Fact]
        public async void Should_benefit_in_descending_order_when_category_matches()
        {
            fixture.Factory.ClearAllEntities();

            var client = fixture.Factory.CreateClient();

            var promotion = await new PromotionBuilder()
                                  .QualifiedBy(new IsCurrentDayConditionBuilder()
                                      .Day(DateTime.Now.Day))
                                  .BenefitBy(new CartItemsMatchingInCategoryPercentageDiscountActionBuilder()
                                      .PercentageOff(50)
                                      .ForCategory("Laptops")
                                      .Operator(Operator.Equal)
                                      .NumberOfProducts(2)
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
            AwardedAdjustment adjustment = line.Adjustments.Single(x => x.AwardingBlock == nameof(CartItemsMatchingInCategoryPercentageDiscountAction));
            Assert.Equal(-25, adjustment.Adjustment.Amount);

            // Subtotal = 65, Tax is 10% = 6.5, Fulfillment fee = 5
            Assert.Equal(76.5m, resultCart.Totals.GrandTotal.Amount);
        }
        
        [Fact]
        public async void Should_benefit_multiple_times_when_within_action_limit()
        {
            fixture.Factory.ClearAllEntities();

            var client = fixture.Factory.CreateClient();

            var promotion = await new PromotionBuilder()
                                  .QualifiedBy(new IsCurrentDayConditionBuilder()
                                      .Day(DateTime.Now.Day))
                                  .BenefitBy(new CartItemsMatchingInCategoryPercentageDiscountActionBuilder()
                                      .PercentageOff(50)
                                      .ForCategory("Laptops")
                                      .Operator(Operator.Equal)
                                      .NumberOfProducts(2)
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
                                 new LineBuilder().IdentifiedBy("002").Quantity(1).InCategory("435345345").Price(50))
                             .Build();

            fixture.Factory.AddEntity(cart);

            var resultCart = await client.GetJsonAsync<Cart>("api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            var firstLine = resultCart.Lines.Single(x => x.Id == "001");
            AwardedAdjustment adjustment = firstLine.Adjustments.Single(x => x.AwardingBlock == nameof(CartItemsMatchingInCategoryPercentageDiscountAction));
            Assert.Equal(-20, adjustment.Adjustment.Amount);

            var secondLine = resultCart.Lines.Single(x => x.Id == "002");
            AwardedAdjustment secondAdjustment = secondLine.Adjustments.Single(x => x.AwardingBlock == nameof(CartItemsMatchingInCategoryPercentageDiscountAction));
            Assert.Equal(-25, secondAdjustment.Adjustment.Amount);

            // Subtotal = 45, Tax is 10% = 4.5, Fulfillment fee = 5
            Assert.Equal(54.5m, resultCart.Totals.GrandTotal.Amount);
        }

        [Theory]
        [InlineData(Operator.Equal, 10, 10, true)]
        [InlineData(Operator.Equal, 10, 9, false)]
        [InlineData(Operator.GreaterThanOrEqual, 10, 10, true)]
        [InlineData(Operator.GreaterThanOrEqual, 10, 11, true)]
        [InlineData(Operator.GreaterThanOrEqual, 10, 9, false)]
        [InlineData(Operator.GreaterThan, 10, 11, true)]
        [InlineData(Operator.GreaterThan, 10, 10, false)]
        [InlineData(Operator.LessThanOrEqual, 10, 10, true)]
        [InlineData(Operator.LessThanOrEqual, 10, 9, true)]
        [InlineData(Operator.LessThanOrEqual, 10, 11, false)]
        [InlineData(Operator.LessThan, 10, 9, true)]
        [InlineData(Operator.LessThan, 10, 10, false)]
        [InlineData(Operator.NotEqual, 9, 10, true)]
        [InlineData(Operator.NotEqual, 10, 10, false)]
        public async void Should_match_operator(Operator @operator, int numberOfProductsInPromotion, int numberOfProductsInCart, bool shouldQualify)
        {
            fixture.Factory.ClearAllEntities();

            var client = fixture.Factory.CreateClient();

            var promotion = await new PromotionBuilder()
                                  .QualifiedBy(new IsCurrentDayConditionBuilder()
                                      .Day(DateTime.Now.Day))
                                  .BenefitBy(new CartItemsMatchingInCategoryPercentageDiscountActionBuilder()
                                      .PercentageOff(50)
                                      .ForCategory("Laptops")
                                      .Operator(@operator)
                                      .NumberOfProducts(numberOfProductsInPromotion)
                                      .ApplyActionTo(ApplicationOrder.Ascending)
                                      .ActionLimit(numberOfProductsInPromotion))
                                  .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            fixture.Factory.AddEntity(new Category
            {
                Id = "Laptops".ToEntityId<Category>(),
                SitecoreId = "435345345"
            });

            var cart = await new CartBuilder()
                             .WithLines(new LineBuilder().IdentifiedBy("001").Quantity(numberOfProductsInCart).InCategory("435345345").Price(40))
                             .Build();

            fixture.Factory.AddEntity(cart);

            var resultCart = await client.GetJsonAsync<Cart>("api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            var line = resultCart.Lines.Single(x => x.Id == "001");

            if (shouldQualify)
            {
                Assert.Contains(line.Adjustments, x => x.AwardingBlock == nameof(CartItemsMatchingInCategoryPercentageDiscountAction));
            } 
            else
            {
                Assert.DoesNotContain(line.Adjustments, x => x.AwardingBlock == nameof(CartItemsMatchingInCategoryPercentageDiscountAction));
            }
        }
    }
}
