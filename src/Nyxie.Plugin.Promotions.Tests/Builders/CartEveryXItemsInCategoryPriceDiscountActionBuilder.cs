using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Nyxie.Plugin.Promotions.Tests.Builders
{
    public class CartEveryXItemsInCategoryPriceDiscountActionBuilder : IBenefitBuilder
    {
        private int actionLimit;
        private decimal amountOff = 10;
        private ApplicationOrder applicationOrder;
        private string category = "";
        private string includeSubCategories = "false";
        private decimal itemsToAward;
        private decimal itemsToPurchase;

        public ActionModel Build()
        {
            return new ActionModel
            {
                Name = "Ny_CartEveryXItemsInCategoryPriceDiscountAction",
                LibraryId = "Ny_CartEveryXItemsInCategoryPriceDiscountAction",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Ny_AmountOff",
                        Value = amountOff.ToString()
                    },
                    new PropertyModel
                    {
                        Name = "Ny_ItemsToAward",
                        Value = itemsToAward.ToString()
                    },
                    new PropertyModel
                    {
                        Name = "Ny_ItemsToPurchase",
                        Value = itemsToPurchase.ToString()
                    },
                    new PropertyModel
                    {
                        Name = "Ny_ApplyActionTo",
                        Value = applicationOrder.Name
                    },
                    new PropertyModel
                    {
                        Name = "Ny_ActionLimit",
                        Value = actionLimit.ToString()
                    },
                    new PropertyModel
                    {
                        Name = "Ny_SpecificCategory",
                        Value = category
                    },
                    new PropertyModel
                    {
                        Name = "Ny_IncludeSubCategories",
                        Value = includeSubCategories
                    }
                }
            };
        }

        public CartEveryXItemsInCategoryPriceDiscountActionBuilder AmountOff(decimal value)
        {
            amountOff = value;
            return this;
        }

        public CartEveryXItemsInCategoryPriceDiscountActionBuilder ItemsToAward(decimal value)
        {
            itemsToAward = value;
            return this;
        }

        public CartEveryXItemsInCategoryPriceDiscountActionBuilder ItemsToPurchase(decimal value)
        {
            itemsToPurchase = value;
            return this;
        }

        public CartEveryXItemsInCategoryPriceDiscountActionBuilder ActionLimit(int limit)
        {
            actionLimit = limit;
            return this;
        }

        public CartEveryXItemsInCategoryPriceDiscountActionBuilder ApplyActionTo(ApplicationOrder applicationOrder)
        {
            this.applicationOrder = applicationOrder;
            return this;
        }

        public CartEveryXItemsInCategoryPriceDiscountActionBuilder ForCategory(string category)
        {
            this.category = category;
            return this;
        }

        public CartEveryXItemsInCategoryPriceDiscountActionBuilder IncludeSubCategories()
        {
            includeSubCategories = "true";
            return this;
        }

        public CartEveryXItemsInCategoryPriceDiscountActionBuilder DoesNotIncludeSubCategories()
        {
            includeSubCategories = "false";
            return this;
        }
    }
}
