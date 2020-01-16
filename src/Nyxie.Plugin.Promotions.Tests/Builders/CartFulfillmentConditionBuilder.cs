using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Nyxie.Plugin.Promotions.Tests.Builders
{
    public class CartFulfillmentConditionBuilder : IQualificationBuilder
    {
        private string comparer = "Sitecore.Framework.Rules.StringEqualityOperator";
        private string fulfillmentMethodName = "Standard";

        public ConditionModel Build()
        {
            return new ConditionModel
            {
                Name = "Hc_CartFulfillmentCondition",
                LibraryId = "Hc_CartFulfillmentCondition",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Hc_BasicStringCompare",
                        Value = comparer
                    },
                    new PropertyModel
                    {
                        Name = "Hc_SpecificFulfillment",
                        Value = fulfillmentMethodName
                    }
                }
            };
        }

        public CartFulfillmentConditionBuilder WithValue(string fulfillmentMethodName)
        {
            this.fulfillmentMethodName = fulfillmentMethodName;
            return this;
        }

        public CartFulfillmentConditionBuilder Equal()
        {
            comparer = "Sitecore.Framework.Rules.StringEqualityOperator";
            return this;
        }

        public CartFulfillmentConditionBuilder NotEqual()
        {
            comparer = "Sitecore.Framework.Rules.StringNotEqualityOperator";
            return this;
        }
    }
}
