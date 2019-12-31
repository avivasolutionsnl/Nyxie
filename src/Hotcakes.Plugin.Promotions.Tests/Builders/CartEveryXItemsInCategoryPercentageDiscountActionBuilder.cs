using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Hotcakes.Plugin.Promotions.Tests.Builders
{
    public class CartEveryXItemsInCategoryPercentageDiscountActionBuilder : IBenefitBuilder
    {
        private decimal percentageOff = 10;
        private decimal itemsToAward;
        private decimal itemsToPurchase;
        private int actionLimit;
        private ApplicationOrder applicationOrder;
        private string category = "";
        private string includeSubCategories = "false";

        public CartEveryXItemsInCategoryPercentageDiscountActionBuilder PercentageOff(decimal value)
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
                Name = "Hc_CartEveryXItemsInCategoryPercentageDiscountAction",
                LibraryId = "Hc_CartEveryXItemsInCategoryPercentageDiscountAction",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Hc_PercentageOff",
                        Value = percentageOff.ToString()
                    },
                    new PropertyModel
                    {
                        Name = "Hc_ItemsToAward",
                        Value = itemsToAward.ToString()
                    },
                    new PropertyModel
                    {
                        Name = "Hc_ItemsToPurchase",
                        Value = itemsToPurchase.ToString()
                    },
                    new PropertyModel
                    {
                        Name = "Hc_ApplyActionTo",
                        Value = applicationOrder.Name
                    },
                    new PropertyModel
                    {
                        Name = "Hc_ActionLimit",
                        Value = actionLimit.ToString()
                    },
                    new PropertyModel
                    {
                        Name = "Hc_SpecificCategory",
                        Value = category
                    },
                    new PropertyModel
                    {
                        Name = "Hc_IncludeSubCategories",
                        Value = includeSubCategories
                    }
                }
            };
        }
    }
}
