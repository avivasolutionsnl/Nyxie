using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Promethium.Plugin.Promotions.Tests.Builders
{
    public class CartEveryXItemsInCategoryPercentageDiscountActionBuilder : IBenefitBuilder
    {
        private string percentageOff = "10";
        private decimal itemsToAward;
        private decimal itemsToPurchase;
        private int actionLimit;
        private ApplicationOrder applicationOrder;
        private string category = "";
        private string includeSubCategories = "false";

        public CartEveryXItemsInCategoryPercentageDiscountActionBuilder PercentageOff(string value)
        {
            percentageOff = value;
            return this;
        }
        
        public CartEveryXItemsInCategoryPercentageDiscountActionBuilder ItemsToAward(decimal value)
        {
            itemsToAward = value;
            return this;
        }

        public CartEveryXItemsInCategoryPercentageDiscountActionBuilder ItemsToPurchase(decimal value)
        {
            itemsToPurchase = value;
            return this;
        }

        public CartEveryXItemsInCategoryPercentageDiscountActionBuilder ActionLimit(int limit)
        {
            actionLimit = limit;
            return this;
        }

        public CartEveryXItemsInCategoryPercentageDiscountActionBuilder ApplyActionTo(ApplicationOrder applicationOrder)
        {
            this.applicationOrder = applicationOrder;
            return this;
        }

        public CartEveryXItemsInCategoryPercentageDiscountActionBuilder ForCategory(string category)
        {
            this.category = category;
            return this;
        }

        public CartEveryXItemsInCategoryPercentageDiscountActionBuilder IncludeSubCategories()
        {
            includeSubCategories = "true";
            return this;
        }

        public CartEveryXItemsInCategoryPercentageDiscountActionBuilder DoesNotIncludeSubCategories()
        {
            includeSubCategories = "false";
            return this;
        }

        public ActionModel Build()
        {
            return new ActionModel
            {
                Name = "Pm_CartEveryXItemsInCategoryPercentageDiscountAction",
                LibraryId = "Pm_CartEveryXItemsInCategoryPercentageDiscountAction",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Pm_PercentageOff",
                        Value = percentageOff
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
