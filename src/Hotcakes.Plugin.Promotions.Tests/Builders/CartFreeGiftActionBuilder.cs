using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Hotcakes.Plugin.Promotions.Tests.Builders
{
    public class CartFreeGiftActionBuilder : IBenefitBuilder
    {
        private decimal quantity = 1;
        private string targetId;

        public CartFreeGiftActionBuilder Quantity(decimal quantity)
        {
            this.quantity = quantity;
            return this;
        }
        
        public CartFreeGiftActionBuilder Gift(string targetId)
        {
            this.targetId = targetId;
            return this;
        }

        public ActionModel Build()
        {
            return new ActionModel
            {
                Name = "Pm_CartFreeGiftAction",
                LibraryId = "Pm_CartFreeGiftAction",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Pm_Quantity",
                        Value = quantity.ToString()
                    },
                    new PropertyModel
                    {
                        Name = "TargetItemId",
                        Value = targetId
                    }
                }
            };
        }
    }
}
