using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Promethium.Plugin.Promotions.Tests.Builders
{
    public class CartSubtotalPercentOffActionBuilder : IBenefitBuilder
    {
        private decimal percentOff = 10;

        public CartSubtotalPercentOffActionBuilder PercentOff(decimal value)
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
                        Value = percentOff.ToString()
                    }
                }
            };
        }
    }
}
