using System.Net.Http;

using Hotcakes.Plugin.Promotions.Tests.Builders;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Promotions;

using Xunit;
using Xunit.Abstractions;

namespace Hotcakes.Plugin.Promotions.Tests.Conditions
{
    [Collection("Engine collection")]
    public class CartProductAmountInCategoryConditionTests
    {
        private readonly EngineFixture fixture;

        public CartProductAmountInCategoryConditionTests(EngineFixture engineFixture, ITestOutputHelper testOutputHelper)
        {
            engineFixture.SetOutput(testOutputHelper);
            fixture = engineFixture;
        }

        [Fact]
        public async void Should_qualify_when_category_matches()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new CartProductAmountInCategoryConditionBuilder()
                                                     .Operator(Operator.Equal)
                                                     .NumberOfProducts(1)
                                                     .ForCategory("Laptops"))
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
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
                                         .Quantity(1)
                                         .InCategory("435345345"))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            Assert.Contains(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }

        [Fact]
        public async void Should_not_qualify_when_category_does_not_match()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new CartProductAmountInCategoryConditionBuilder()
                                                     .Operator(Operator.Equal)
                                                     .NumberOfProducts(1)
                                                     .ForCategory("Tablets"))
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
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
                                         .Quantity(1)
                                         .InCategory("435345345"))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            Assert.DoesNotContain(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }

        [Fact]
        public async void Should_qualify_when_includes_sub_category()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new CartProductAmountInCategoryConditionBuilder()
                                                     .Operator(Operator.Equal)
                                                     .NumberOfProducts(1)
                                                     .ForCategory("Laptops")
                                                     .IncludeSubCategories())
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
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
                                         .Quantity(1)
                                         .InCategory("/435345345/subcategory"))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            Assert.Contains(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }

        [Fact]
        public async void Should_not_qualify_when_does_not_includes_sub_category()
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new CartProductAmountInCategoryConditionBuilder()
                                                     .Operator(Operator.Equal)
                                                     .NumberOfProducts(1)
                                                     .ForCategory("Laptops")
                                                     .DoesNotIncludeSubCategories())
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
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
                                         .Quantity(1)
                                         .InCategory("/435345345/subcategory"))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            Assert.DoesNotContain(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
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
        public async void Should_match_operator(Operator @operator, int numberOfProductsInPromotion, int numberOfProductsInCart,
            bool shouldQualify)
        {
            fixture.Factory.ClearAllEntities();

            HttpClient client = fixture.Factory.CreateClient();

            Promotion promotion = await new PromotionBuilder()
                                        .QualifiedBy(new CartProductAmountInCategoryConditionBuilder()
                                                     .Operator(@operator)
                                                     .NumberOfProducts(numberOfProductsInPromotion)
                                                     .ForCategory("Laptops"))
                                        .BenefitBy(new CartSubtotalPercentOffActionBuilder()
                                            .PercentOff(10))
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
                                         .Quantity(numberOfProductsInCart)
                                         .InCategory("435345345"))
                              .Build();

            fixture.Factory.AddEntity(cart);

            Cart resultCart =
                await client.GetJsonAsync<Cart>(
                    "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");

            if (shouldQualify)
                Assert.Contains(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
            else
                Assert.DoesNotContain(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }
    }
}
