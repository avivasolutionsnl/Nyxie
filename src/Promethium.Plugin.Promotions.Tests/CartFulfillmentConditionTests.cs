using System;
using System.Collections.Generic;
using System.Net.Http;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Commerce.Plugin.Rules;

using Xunit;
using Xunit.Abstractions;

namespace Promethium.Plugin.Promotions.Tests
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
        public async void Should_qualify()
        {
            var client = fixture.Factory.CreateClient();

            var promotion = new Promotion
            {
                Id = "P001",
                ValidFrom = DateTimeOffset.UtcNow.AddDays(-10),
                ValidTo = DateTimeOffset.UtcNow.AddDays(10),
                IsPersisted = true,
                Published = true,
                Name = "My promotion"
            };

            promotion.AddPolicies(new PromotionQualificationsPolicy
            {
                Qualifications = new[]
                {
                    new ConditionModel
                    {
                        Name = "Pm_CartFulfillmentCondition",
                        LibraryId = "Pm_CartFulfillmentCondition",
                        Properties = new List<PropertyModel>
                        {
                            new PropertyModel
                            {
                                Name = "Pm_BasicStringCompare",
                                Value = "Sitecore.Framework.Rules.StringEqualityOperator"
                            },
                            new PropertyModel
                            {
                                Name = "Pm_SpecificFulfillment",
                                Value = "Standard"
                            }
                        }
                    }
                }
            }, new PromotionBenefitsPolicy
            {
                Benefits = new[]
                {
                    new ActionModel
                    {
                        Name = "CartSubtotalPercentOffAction",
                        LibraryId = "CartSubtotalPercentOffAction",
                        Properties = new List<PropertyModel>
                        {
                            new PropertyModel
                            {
                                Name = "PercentOff",
                                Value = "10"
                            }
                        }
                    }
                }
            });

            promotion.AddComponents(new PromotionRulesComponent(), new ApprovalComponent("Approved"));

            using (var scope = fixture.Factory.Server.Host.Services.CreateScope())
            {
                var block  = scope.ServiceProvider.GetRequiredService<BuildPromotionQualifyingRuleBlock>();

                promotion = await block.Run(promotion, fixture.Factory.CreateCommerceContext().PipelineContext);

                var applyingBlock = scope.ServiceProvider.GetRequiredService<BuildPromotionApplyingRuleBlock>();

                promotion = await applyingBlock.Run(promotion, fixture.Factory.CreateCommerceContext().PipelineContext);
            }

            fixture.Factory.AddEntityToList(promotion, CommerceEntity.ListName<Promotion>());
            fixture.Factory.AddEntity(promotion);

            var cart = new Cart
            {
                Id = "Cart01",
                DateUpdated = DateTimeOffset.UtcNow
            };
            cart.AddComponents(new ContactComponent
            {
                Language = "en"
            }, new FulfillmentComponent
            {
                FulfillmentMethod = new EntityReference("001", "Standard")
            });

            cart.Lines.Add(new CartLineComponent
            {
                Quantity = 1,
                ItemId = "001",
                Policies = { new PurchaseOptionMoneyPolicy
                {
                    SellPrice = new Money(33),
                }}
            });

            cart.AddPolicies(new CalculateCartPolicy{ AlwaysCalculate = true});
            fixture.Factory.AddEntity(cart);

            var message = new HttpRequestMessage(HttpMethod.Get, "api/Carts('Cart01')?$expand=Lines($expand=CartLineComponents($expand=ChildComponents)),Components");
         
            var response = await client.SendAsync(message);

            Assert.True(response.IsSuccessStatusCode);

            var responseAsString = await response.Content.ReadAsStringAsync();

            var resultCart =  JsonConvert.DeserializeObject<Cart>(responseAsString);

            Assert.Contains(resultCart.Adjustments, c => c.AwardingBlock == nameof(CartSubtotalPercentOffAction));
        }


    }

}
