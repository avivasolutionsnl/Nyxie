using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Promethium.Plugin.Promotions.Tests.Builders
{
    public class CartAmountOffFulfillmentActionBuilder : IBenefitBuilder
    {
        private decimal amountOff = 10;

        public CartAmountOffFulfillmentActionBuilder AmountOff(decimal value)
        {
            amountOff = value;
            return this;
        }

        public ActionModel Build()
        {
            return new ActionModel
            {
                Name = "Pm_CartAmountOffFulfillmentAction",
                LibraryId = "Pm_CartAmountOffFulfillmentAction",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Pm_SpecificAmount",
                        Value = amountOff.ToString()
                    }
                }
            };
        }
    }
}
