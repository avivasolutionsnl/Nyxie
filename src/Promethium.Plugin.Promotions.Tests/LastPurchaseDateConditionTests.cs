using System;
using System.Collections.Generic;
using System.Globalization;

using Promethium.Plugin.Promotions.Tests.Builders;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Promotions;

using Xunit;
using Xunit.Abstractions;

namespace Promethium.Plugin.Promotions.Tests
{
    [Collection("Engine collection")]
    public class LastPurchaseDateConditionTests
    { 
        private readonly EngineFixture fixture;

        public LastPurchaseDateConditionTests(EngineFixture engineFixture, ITestOutputHelper testOutputHelper)
        {
            engineFixture.SetOutput(testOutputHelper);
            fixture = engineFixture;
        }

        [Fact]
        public async void Should_qualify_when_customer_is_registered_and_date_matches()
        {
            fixture.Factory.ClearAllEntities();

            var client = fixture.Factory.CreateClient();

            var dateTime = new DateTimeOffset(new DateTime(2019, 12, 22));
            var customerId = Guid.NewGuid();

            var order = new OrderBuilder().PlacedOn(dateTime).Build();

            fixture.Factory.AddEntityToList(order,
                string.Format(CultureInfo.InvariantCulture, "Orders-ByCustomer-{0}", customerId));

            var promotion = await new PromotionBuilder()
                                  .QualifiedBy(new LastPurchaseDateConditionBuilder()
                                      .Operator(Operator.Equal)
                                      .Date(dateTime))
                                  .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                      .PercentOff("10"))
                                  .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            var cart = await new CartBuilder().Build();

            fixture.Factory.AddEntity(cart);
            
            var resultCart = await client.GetJsonAsync<Cart>(
                "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components",
                new Dictionary<string, string>
                {
                    { "IsRegistered", "true" },
                    { "CustomerId", customerId.ToString() }
                });
            
            Assert.Contains(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }

        [Fact]
        public async void Should_not_qualify_when_there_are_no_orders()
        {
            fixture.Factory.ClearAllEntities();

            var client = fixture.Factory.CreateClient();

            var dateTime = new DateTimeOffset(new DateTime(2019, 12, 22));
            var customerId = Guid.NewGuid();

            var promotion = await new PromotionBuilder()
                                  .QualifiedBy(new LastPurchaseDateConditionBuilder()
                                               .Operator(Operator.Equal)
                                               .Date(dateTime))
                                  .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                      .PercentOff("10"))
                                  .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            var cart = await new CartBuilder().Build();

            fixture.Factory.AddEntity(cart);

            var resultCart = await client.GetJsonAsync<Cart>(
                "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components",
                new Dictionary<string, string>
                {
                    { "IsRegistered", "true" },
                    { "CustomerId", customerId.ToString() }
                });

            Assert.DoesNotContain(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }


        [Fact]
        public async void Should_not_qualify_when_customer_is_not_registered()
        {
            fixture.Factory.ClearAllEntities();

            var client = fixture.Factory.CreateClient();

            var dateTime = new DateTimeOffset(new DateTime(2019, 12, 22));

            var promotion = await new PromotionBuilder()
                                  .QualifiedBy(new LastPurchaseDateConditionBuilder()
                                               .Operator(Operator.Equal)
                                               .Date(dateTime))
                                  .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                      .PercentOff("10"))
                                  .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            var cart = await new CartBuilder().Build();

            fixture.Factory.AddEntity(cart);

            var resultCart = await client.GetJsonAsync<Cart>(
                "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components",
                new Dictionary<string, string>
                {
                    { "IsRegistered", "false" }
                });

            Assert.DoesNotContain(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async void Should_match_operator(Operator @operator, DateTimeOffset promotionDate, DateTimeOffset firstOrderDate, bool shouldQualify)
        {
            fixture.Factory.ClearAllEntities();

            var client = fixture.Factory.CreateClient();

            var customerId = Guid.NewGuid();

            var order = new OrderBuilder().PlacedOn(firstOrderDate).Build();

            var earlierOrder = new OrderBuilder().PlacedOn(firstOrderDate.AddDays(-100)).Build();

            fixture.Factory.AddEntitiesToList(string.Format(CultureInfo.InvariantCulture, "Orders-ByCustomer-{0}", customerId),
                order, earlierOrder);

            var promotion = await new PromotionBuilder()
                                  .QualifiedBy(new LastPurchaseDateConditionBuilder()
                                               .Operator(@operator)
                                               .Date(promotionDate))
                                  .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                      .PercentOff("10"))
                                  .Build(fixture.Factory);

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            var cart = await new CartBuilder().Build();

            fixture.Factory.AddEntity(cart);

            var resultCart = await client.GetJsonAsync<Cart>(
                "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components",
                new Dictionary<string, string>
                {
                    { "IsRegistered", "true" },
                    { "CustomerId", customerId.ToString() }
                });

            if (shouldQualify)
            {
                Assert.Contains(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
            }
            else
            {
                Assert.DoesNotContain(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
            }
        }

        public static IEnumerable<object[]> Data => new List<object[]>
            {
                new object[] { Operator.Equal, new DateTimeOffset(new DateTime(2019, 12, 22)), new DateTimeOffset(new DateTime(2019, 12, 22)), true },
                new object[] { Operator.Equal, new DateTimeOffset(new DateTime(2019, 12, 23)), new DateTimeOffset(new DateTime(2019, 12, 22)), false },
                new object[] { Operator.NotEqual, new DateTimeOffset(new DateTime(2019, 12, 23)), new DateTimeOffset(new DateTime(2019, 12, 22)), true },
                new object[] { Operator.NotEqual, new DateTimeOffset(new DateTime(2019, 12, 22)), new DateTimeOffset(new DateTime(2019, 12, 22)), false },
                new object[] { Operator.GreaterThanOrEqual, new DateTimeOffset(new DateTime(2019, 12, 22)), new DateTimeOffset(new DateTime(2019, 12, 22)), true },
                new object[] { Operator.GreaterThanOrEqual, new DateTimeOffset(new DateTime(2019, 12, 22)), new DateTimeOffset(new DateTime(2019, 12, 23)), true },
                new object[] { Operator.GreaterThanOrEqual, new DateTimeOffset(new DateTime(2019, 12, 22)), new DateTimeOffset(new DateTime(2019, 12, 21)), false },
                new object[] { Operator.GreaterThan, new DateTimeOffset(new DateTime(2019, 12, 22)), new DateTimeOffset(new DateTime(2019, 12, 21)), false },
                new object[] { Operator.GreaterThan, new DateTimeOffset(new DateTime(2019, 12, 22)), new DateTimeOffset(new DateTime(2019, 12, 22)), false },
                new object[] { Operator.GreaterThan, new DateTimeOffset(new DateTime(2019, 12, 22)), new DateTimeOffset(new DateTime(2019, 12, 23)), true},
                new object[] { Operator.LessThanOrEqual, new DateTimeOffset(new DateTime(2019, 12, 22)), new DateTimeOffset(new DateTime(2019, 12, 22)), true},
                new object[] { Operator.LessThanOrEqual, new DateTimeOffset(new DateTime(2019, 12, 22)), new DateTimeOffset(new DateTime(2019, 12, 21)), true},
                new object[] { Operator.LessThanOrEqual, new DateTimeOffset(new DateTime(2019, 12, 22)), new DateTimeOffset(new DateTime(2019, 12, 23)), false},
                new object[] { Operator.LessThan, new DateTimeOffset(new DateTime(2019, 12, 22)), new DateTimeOffset(new DateTime(2019, 12, 21)), true},
                new object[] { Operator.LessThan, new DateTimeOffset(new DateTime(2019, 12, 22)), new DateTimeOffset(new DateTime(2019, 12, 22)), false},
                new object[] { Operator.LessThan, new DateTimeOffset(new DateTime(2019, 12, 22)), new DateTimeOffset(new DateTime(2019, 12, 23)), false}
            };
    }
}
