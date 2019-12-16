using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FakeItEasy;

using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

using Promethium.Plugin.Promotions.Conditions;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Framework.Rules;

using Xunit;

namespace Promethium.Plugin.Promotions.Tests
{
    public class CartFulfillmentConditionTests
    {
        [Fact]
        public void Should_qualify()
        {
            var condition = new CartFulfillmentCondition
            {
                Pm_BasicStringCompare = new LiteralRuleValue<string>("Sitecore.Framework.Rules.StringEqualityOperator"),
                Pm_SpecificFulfillment = new LiteralRuleValue<string>("GiftCard")
            };

            var commerceContext = new CommerceContext(A.Fake<ILogger>(), new TelemetryClient(),
                A.Fake<IGetLocalizableMessagePipeline>());

            var cart = new Cart
            {
                Lines = new[] {new CartLineComponent() }
            };

            cart.AddComponents(new FulfillmentComponent
            {
                FulfillmentMethod = new EntityReference("001", "GiftCard")
            });

            commerceContext.AddObject(cart);


            bool result = condition.Evaluate(
                new RuleExecutionContext(new FactProvider(new[]
                {
                    new LiteralFactResolver(commerceContext)
                })));

            Assert.True(result);
        }
    }
}
