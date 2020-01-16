using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Nyxie.Plugin.Promotions.Tests.Builders
{
    public class CartEveryXItemsInCategoryPercentageDiscountActionBuilder : IBenefitBuilder
    {
        private int actionLimit;
        private ApplicationOrder applicationOrder;
        private string category = "";
        private string includeSubCategories = "false";
        private decimal itemsToAward;
        private decimal itemsToPurchase;
        private decimal percentageOff = 10;

        public ActionModel Build()
        {
            return new ActionModel
            {
                Name = "Ny_CartEveryXItemsInCategoryPercentageDiscountAction",
                LibraryId = "Ny_CartEveryXItemsInCategoryPercentageDiscountAction",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Ny_PercentageOff",
                        Value = percentageOff.ToString()
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
    }
}
