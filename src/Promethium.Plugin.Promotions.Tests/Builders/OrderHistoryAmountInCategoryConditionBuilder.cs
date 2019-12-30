using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Promethium.Plugin.Promotions.Tests.Builders
{
    public class OrderHistoryAmountInCategoryConditionBuilder : IQualificationBuilder
    {
        private string category = "";
        private int numberOfProducts = 0;
        private string includeSubCategories = "false";
        private Operator @operator = Builders.Operator.Equal;

        public OrderHistoryAmountInCategoryConditionBuilder ForCategory(string category)
        {
            this.category = category;
            return this;
        }

        public OrderHistoryAmountInCategoryConditionBuilder NumberOfProducts(int numberOfProducts)
        {
            this.numberOfProducts = numberOfProducts;
            return this;
        }

        public OrderHistoryAmountInCategoryConditionBuilder Operator(Operator @operator)
        {
            this.@operator = @operator;
            return this;
        }

        public OrderHistoryAmountInCategoryConditionBuilder IncludeSubCategories()
        {
            includeSubCategories = "true";
            return this;
        }

        public OrderHistoryAmountInCategoryConditionBuilder DoesNotIncludeSubCategories()
        {
            includeSubCategories = "false";
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
                Name = "Pm_OrderHistoryAmountInCategoryCondition",
                LibraryId = "Pm_OrderHistoryAmountInCategoryCondition",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Pm_SpecificCategory",
                        Value = category
                    },
                    new PropertyModel
                    {
                        Name = "Pm_Compares",
                        Value = comparer,
                        IsOperator = true
                    },
                    new PropertyModel
                    {
                        Name = "Pm_SpecificValue",
                        Value = numberOfProducts.ToString()
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
