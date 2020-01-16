using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Nyxie.Plugin.Promotions.Tests.Builders
{
    public class CartFreeGiftActionBuilder : IBenefitBuilder
    {
        private decimal quantity = 1;
        private string targetId;

        public ActionModel Build()
        {
            return new ActionModel
            {
                Name = "Ny_CartFreeGiftAction",
                LibraryId = "Ny_CartFreeGiftAction",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Ny_Quantity",
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
    }
}
