using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Nyxie.Plugin.Promotions.Tests.Builders
{
    public class CartPaymentConditionBuilder : IQualificationBuilder
    {
        private string comparer = "Sitecore.Framework.Rules.StringEqualityOperator";
        private string paymentMethodName = "Standard";

        public ConditionModel Build()
        {
            return new ConditionModel
            {
                Name = "Ny_CartPaymentCondition",
                LibraryId = "Ny_CartPaymentCondition",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Ny_BasicStringCompare",
                        Value = comparer
                    },
                    new PropertyModel
                    {
                        Name = "Ny_SpecificPayment",
                        Value = paymentMethodName
                    }
                }
            };
        }

        public CartPaymentConditionBuilder WithValue(string paymentMethodName)
        {
            this.paymentMethodName = paymentMethodName;
            return this;
        }

        public CartPaymentConditionBuilder Equal()
        {
            comparer = "Sitecore.Framework.Rules.StringEqualityOperator";
            return this;
        }

        public CartPaymentConditionBuilder NotEqual()
        {
            comparer = "Sitecore.Framework.Rules.StringNotEqualityOperator";
            return this;
        }
    }
}
