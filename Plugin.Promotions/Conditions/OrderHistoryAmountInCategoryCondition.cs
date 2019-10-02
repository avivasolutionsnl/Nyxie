using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Rules;
using System.Linq;

namespace Promethium.Plugin.Promotions.Conditions
{
    /// <summary>
    /// A SiteCore Commerce condition for the qualification
    /// "Order history contains [compares] [specific value] products in the [specific] category"
    /// </summary>
    [EntityIdentifier("Promethium_" + nameof(OrderHistoryAmountInCategoryCondition))]
    public class OrderHistoryAmountInCategoryCondition : ICustomerCondition
    {
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;

        public OrderHistoryAmountInCategoryCondition(FindEntitiesInListCommand findEntitiesInListCommand)
        {
            _findEntitiesInListCommand = findEntitiesInListCommand;
        }

        public IBinaryOperator<decimal, decimal> Promethium_Compares { get; set; }

        public IRuleValue<decimal> Promethium_SpecificValue { get; set; }

        public IRuleValue<string> Promethium_SpecificCategory { get; set; }

        public IRuleValue<bool> Promethium_IncludeSubCategories { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            //Get configuration
            var specificCategory = Promethium_SpecificCategory.Yield(context);
            var specificValue = Promethium_SpecificValue.Yield(context);
            var includeSubCategories = Promethium_IncludeSubCategories.Yield(context);
            if (string.IsNullOrEmpty(specificCategory) || specificValue == 0 || Promethium_Compares == null)
            {
                return false;
            }

            //Get Data
            if (!context.GetOrderHistory(_findEntitiesInListCommand, specificCategory, includeSubCategories, out var categoryLines))
            {
                return false;
            }

            //Validate data against configuration
            var productAmount = categoryLines.Sum(x => x.Quantity);
            return Promethium_Compares.Evaluate(productAmount, specificValue);
        }
    }
}
