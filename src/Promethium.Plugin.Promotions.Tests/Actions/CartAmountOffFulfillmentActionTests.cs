using System;
using System.Linq;

using Promethium.Plugin.Promotions.Actions;
using Promethium.Plugin.Promotions.Tests.Builders;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Commerce.Plugin.Promotions;

using Xunit;
using Xunit.Abstractions;

namespace Promethium.Plugin.Promotions.Tests.Actions
{
    [Collection("Engine collection")]
    public class CartAmountOffFulfillmentActionTests
    {
        private readonly EngineFixture fixture;

        public CartAmountOffFulfillmentActionTests(EngineFixture engineFixture, ITestOutputHelper testOutputHelper)
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
                                  .BenefitBy(new CartAmountOffFulfillmentActionBuilder()
                                      .AmountOff(2))
                                  .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            var cart = await new CartBuilder()
                             .WithLines(new LineBuilder().Quantity(1).Price(50))
                             .WithStandardFulfillment() // Default fulfillment fee is 5
                             .Build();

            fixture.Factory.AddEntity(cart);

            var resultCart = await client.GetJsonAsync<Cart>("api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");
            AwardedAdjustment adjustment = resultCart.Adjustments.Single(x => x.AwardingBlock == nameof(CartAmountOffFulfillmentAction));
            Assert.Equal(-2, adjustment.Adjustment.Amount);

            // Subtotal = 50, Tax is 10% = 5, Fulfillment fee = 5 - 2 = 3
            Assert.Equal(58, resultCart.Totals.GrandTotal.Amount);
        }

        [Fact]
        public async void Should_benefit_fulfillment_fee_if_discount_is_higher_than_fee()
        {
            fixture.Factory.ClearAllEntities();

            var client = fixture.Factory.CreateClient();

            var promotion = await new PromotionBuilder()
                                  .QualifiedBy(new IsCurrentDayConditionBuilder()
                                      .Day(DateTime.Now.Day)) 
                                  .BenefitBy(new CartAmountOffFulfillmentActionBuilder()
                                      .AmountOff(8)) // Fee = 5, Discount = 8
                                  .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            var cart = await new CartBuilder()
                             .WithLines(new LineBuilder().Quantity(1).Price(50))
                             .WithStandardFulfillment() // Default fulfillment fee = 5 
                             .Build();

            fixture.Factory.AddEntity(cart);

            var resultCart = await client.GetJsonAsync<Cart>("api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");
            AwardedAdjustment adjustment = resultCart.Adjustments.Single(x => x.AwardingBlock == nameof(CartAmountOffFulfillmentAction));
            Assert.Equal(-5, adjustment.Adjustment.Amount);

            // Subtotal = 50, Tax is 10% = 5, Fulfillment fee = 5 - 5 = 0
            Assert.Equal(55, resultCart.Totals.GrandTotal.Amount);
        }

        [Fact]
        public async void Should_benefit_split_fulfillments()
        {
            fixture.Factory.ClearAllEntities();

            var client = fixture.Factory.CreateClient();

            var promotion = await new PromotionBuilder()
                                  .QualifiedBy(new IsCurrentDayConditionBuilder()
                                      .Day(DateTime.Now.Day)) 
                                  .BenefitBy(new CartAmountOffFulfillmentActionBuilder()
                                      .AmountOff(3)) // Fee = 2 * 2, Discount = 3
                                  .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            var cart = await new CartBuilder()
                             .WithLines(new LineBuilder()
                                        .IdentifiedBy("001")
                                        .Quantity(1)
                                        .WithStandardFulfillment() // Default fulfillment fee per line item is 2
                                        .Price(50), new LineBuilder()
                                                    .IdentifiedBy("002")
                                                    .Quantity(1)
                                                    .WithStandardFulfillment() // Default fulfillment fee per line item is 2
                                                    .Price(50))
                             .WithSplitFulfillment()
                             .Build();

            fixture.Factory.AddEntity(cart);

            var resultCart = await client.GetJsonAsync<Cart>("api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");
            AwardedAdjustment adjustment = resultCart.Adjustments.Single(x => x.AwardingBlock == nameof(CartAmountOffFulfillmentAction));
            Assert.Equal(-3, adjustment.Adjustment.Amount);

            // Subtotal = 100, Tax is 10% = 10, Fulfillment fee = 2*2 - 3 = 1
            Assert.Equal(111, resultCart.Totals.GrandTotal.Amount);
        }
    }
}
