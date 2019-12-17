using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Promethium.Plugin.Promotions.Tests.Builders
{
    public class CartSubtotalPercentOffActionBuilder : IBenefitBuilder
    {
        private string percentOff = "10";

        public CartSubtotalPercentOffActionBuilder PercentOff(string value)
        {
            percentOff = value;
            return this;
        }

        public ActionModel Build()
        {
            return new ActionModel
            {
                Name = "CartSubtotalPercentOffAction",
                LibraryId = "CartSubtotalPercentOffAction",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "PercentOff",
                        Value = percentOff
                    }
                }
            };
        }
    }
}
