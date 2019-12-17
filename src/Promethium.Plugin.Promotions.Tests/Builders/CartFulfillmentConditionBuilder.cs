using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Promethium.Plugin.Promotions.Tests.Builders
{
    public class CartFulfillmentConditionBuilder : IQualificationBuilder
    {
        private string comparer = "Sitecore.Framework.Rules.StringEqualityOperator";
        private string value = "Standard";

        public CartFulfillmentConditionBuilder WithValue(string value)
        {
            this.value = value;
            return this;
        }

        public CartFulfillmentConditionBuilder Equal()
        {
            this.comparer = "Sitecore.Framework.Rules.StringEqualityOperator";
            return this;
        }

        public CartFulfillmentConditionBuilder NotEqual()
        {
            this.comparer = "Sitecore.Framework.Rules.StringNotEqualityOperator";
            return this;
        }

        public ConditionModel Build()
        {
            return new ConditionModel
            {
                Name = "Pm_CartFulfillmentCondition",
                LibraryId = "Pm_CartFulfillmentCondition",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Pm_BasicStringCompare",
                        Value = comparer
                    },
                    new PropertyModel
                    {
                        Name = "Pm_SpecificFulfillment",
                        Value = value
                    }
                }
            };
        }
    }
}
