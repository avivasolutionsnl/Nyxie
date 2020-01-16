using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Nyxie.Plugin.Promotions.Tests.Builders
{
    public class CartItemsMatchingInCategoryPriceDiscountActionBuilder : IBenefitBuilder
    {
        private int actionLimit;
        private decimal amountOff = 10;
        private ApplicationOrder applicationOrder;
        private string category = "";
        private string includeSubCategories = "false";
        private int numberOfProducts;
        private Operator @operator;

        public ActionModel Build()
        {
            string comparer;
            switch (@operator)
            {
                case Builders.Operator.NotEqual:
                    comparer = "Sitecore.Framework.Rules.DecimalNotEqualityOperator";
                    break;
                case Builders.Operator.GreaterThan:
                    comparer = "Sitecore.Framework.Rules.DecimalGreaterThanOperator";
                    break;
                case Builders.Operator.GreaterThanOrEqual:
                    comparer = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator";
                    break;
                case Builders.Operator.LessThanOrEqual:
                    comparer = "Sitecore.Framework.Rules.DecimalLessThanEqualToOperator";
                    break;
                case Builders.Operator.LessThan:
                    comparer = "Sitecore.Framework.Rules.DecimalLessThanOperator";
                    break;
                default:
                    comparer = "Sitecore.Framework.Rules.DecimalEqualityOperator";
                    break;
            }

            return new ActionModel
            {
                Name = "Ny_CartItemsMatchingInCategoryPriceDiscountAction",
                LibraryId = "Ny_CartItemsMatchingInCategoryPriceDiscountAction",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Ny_AmountOff",
                        Value = amountOff.ToString()
                    },
                    new PropertyModel
                    {
                        Name = "Ny_SpecificValue",
                        Value = numberOfProducts.ToString()
                    },
                    new PropertyModel
                    {
                        Name = "Ny_Operator",
                        Value = comparer,
                        IsOperator = true
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

        public CartItemsMatchingInCategoryPriceDiscountActionBuilder AmountOff(decimal amount)
        {
            amountOff = amount;
            return this;
        }

        public CartItemsMatchingInCategoryPriceDiscountActionBuilder Operator(Operator @operator)
        {
            this.@operator = @operator;
            return this;
        }

        public CartItemsMatchingInCategoryPriceDiscountActionBuilder NumberOfProducts(int numberOfProducts)
        {
            this.numberOfProducts = numberOfProducts;
            return this;
        }

        public CartItemsMatchingInCategoryPriceDiscountActionBuilder ActionLimit(int limit)
        {
            actionLimit = limit;
            return this;
        }

        public CartItemsMatchingInCategoryPriceDiscountActionBuilder ApplyActionTo(ApplicationOrder applicationOrder)
        {
            this.applicationOrder = applicationOrder;
            return this;
        }

        public CartItemsMatchingInCategoryPriceDiscountActionBuilder ForCategory(string category)
        {
            this.category = category;
            return this;
        }

        public CartItemsMatchingInCategoryPriceDiscountActionBuilder IncludeSubCategories()
        {
            includeSubCategories = "true";
            return this;
        }

        public CartItemsMatchingInCategoryPriceDiscountActionBuilder DoesNotIncludeSubCategories()
        {
            includeSubCategories = "false";
            return this;
        }
    }
}
