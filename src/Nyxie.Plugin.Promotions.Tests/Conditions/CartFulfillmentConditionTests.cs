using System.Net.Http;

using Nyxie.Plugin.Promotions.Tests.Builders;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Promotions;

using Xunit;
using Xunit.Abstractions;

namespace Nyxie.Plugin.Promotions.Tests.Conditions
{
    [Collection("Engine collection")]
    public class CartFulfillmentConditionTests
    {
        private readonly EngineFixture fixture;

        public CartFulfillmentConditionTests(EngineFixture engineFixture, ITestOutputHelper testOutputHelper)
        {
            engineFixture.SetOutput(testOutputHelper);
            fixture = engineFixture;
        }

        [Fact]
        public async void Should_qualify_when_operator_is_equal_and_fulfillment_method_is_same()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new CartFulfillmentConditionBuilder()
                                                     .Equal()
                                                     .WithValue("Standard"))
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            Cart cart = await new CartBuilder()
                              .WithStandardFulfillment()
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            Assert.Contains(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }

        [Fact]
        public async void Should_not_qualify_when_operator_is_equal_and_fulfillment_method_is_different()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new CartFulfillmentConditionBuilder()
                                                     .Equal()
                                                     .WithValue("Other"))
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            Cart cart = await new CartBuilder()
                              .WithStandardFulfillment()
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            Assert.DoesNotContain(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }

        [Fact]
        public async void Should_qualify_when_operator_is_not_equal_and_fulfillment_method_is_different()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new CartFulfillmentConditionBuilder()
                                                     .NotEqual()
                                                     .WithValue("Other"))
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            Cart cart = await new CartBuilder()
                              .WithStandardFulfillment()
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            Assert.Contains(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }

        [Fact]
        public async void Should_not_qualify_when_operator_is_not_equal_and_fulfillment_method_is_same()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new CartFulfillmentConditionBuilder()
                                                     .NotEqual()
                                                     .WithValue("Standard"))
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
                                        .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            Cart cart = await new CartBuilder()
                              .WithStandardFulfillment()
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            Assert.DoesNotContain(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }
    }
}
