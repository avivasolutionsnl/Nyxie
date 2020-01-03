using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Hotcakes.Plugin.Promotions.Tests.Builders
{
    public class CartProductAmountInCategoryConditionBuilder : IQualificationBuilder
    {
        private string category = "";
        private int numberOfProducts = 0;
        private string includeSubCategories = "false";
        private Operator @operator = Builders.Operator.Equal;

        public CartProductAmountInCategoryConditionBuilder ForCategory(string category)
        {
            this.category = category;
            return this;
        }
        public CartProductAmountInCategoryConditionBuilder IncludeSubCategories()
        {
            includeSubCategories = "true";
            return this;
        }

        public CartProductAmountInCategoryConditionBuilder DoesNotIncludeSubCategories()
        {
            includeSubCategories = "false";
            return this;
        }

        public CartProductAmountInCategoryConditionBuilder NumberOfProducts(int numberOfProducts)
        {
            this.numberOfProducts = numberOfProducts;
            return this;
        }

        public CartProductAmountInCategoryConditionBuilder Operator(Operator @operator)
        {
            this.@operator = @operator;
            return this;
        }
        
        public ConditionModel Build()
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

            return new ConditionModel
            {
                Name = "Hc_CartProductAmountInCategoryCondition",
                LibraryId = "Hc_CartProductAmountInCategoryCondition",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Hc_SpecificCategory",
                        Value = category
                    },
                    new PropertyModel
                    {
                        Name = "Hc_Compares",
                        Value = comparer,
                        IsOperator = true
                    },
                    new PropertyModel
                    {
                        Name = "Hc_SpecificValue",
                        Value = numberOfProducts.ToString()
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
