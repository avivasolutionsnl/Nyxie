using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Promethium.Plugin.Promotions.Tests.Builders
{
    public class CartEveryXItemsInCategoryPriceDiscountActionBuilder : IBenefitBuilder
    {
        private string amountOff = "10";
        private decimal itemsToAward;
        private decimal itemsToPurchase;
        private int actionLimit;
        private ApplicationOrder applicationOrder;
        private string category = "";
        private string includeSubCategories = "false";

        public CartEveryXItemsInCategoryPriceDiscountActionBuilder AmountOff(string value)
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

        public ActionModel Build()
        {
            return new ActionModel
            {
                Name = "Pm_CartEveryXItemsInCategoryPriceDiscountAction",
                LibraryId = "Pm_CartEveryXItemsInCategoryPriceDiscountAction",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Pm_AmountOff",
                        Value = amountOff
                    },
                    new PropertyModel
                    {
                        Name = "Pm_ItemsToAward",
                        Value = itemsToAward.ToString()
                    },
                    new PropertyModel
                    {
                        Name = "Pm_ItemsToPurchase",
                        Value = itemsToPurchase.ToString()
                    },
                    new PropertyModel
                    {
                        Name = "Pm_ApplyActionTo",
                        Value = applicationOrder.Name
                    },
                    new PropertyModel
                    {
                        Name = "Pm_ActionLimit",
                        Value = actionLimit.ToString()
                    },
                    new PropertyModel
                    {
                        Name = "Pm_SpecificCategory",
                        Value = category
                    },
                    new PropertyModel
                    {
                        Name = "Pm_IncludeSubCategories",
                        Value = includeSubCategories
                    }
                }
            };
        }
    }
}
