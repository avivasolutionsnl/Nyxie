using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Hotcakes.Plugin.Promotions.Tests.Builders
{
    public class CartPaymentConditionBuilder : IQualificationBuilder
    {
        private string comparer = "Sitecore.Framework.Rules.StringEqualityOperator";
        private string paymentMethodName = "Standard";

        public CartPaymentConditionBuilder WithValue(string paymentMethodName)
        {
            this.paymentMethodName = paymentMethodName;
            return this;
        }

        public CartPaymentConditionBuilder Equal()
        {
            this.comparer = "Sitecore.Framework.Rules.StringEqualityOperator";
            return this;
        }

        public CartPaymentConditionBuilder NotEqual()
        {
            this.comparer = "Sitecore.Framework.Rules.StringNotEqualityOperator";
            return this;
        }

        public ConditionModel Build()
        {
            return new ConditionModel
            {
                Name = "Hc_CartPaymentCondition",
                LibraryId = "Hc_CartPaymentCondition",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Hc_BasicStringCompare",
                        Value = comparer
                    },
                    new PropertyModel
                    {
                        Name = "Hc_SpecificPayment",
                        Value = paymentMethodName
                    }
                }
            };
        }
    }
}
