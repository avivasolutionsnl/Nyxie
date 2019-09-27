using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Rules;
using System.Linq;

namespace Promethium.Plugin.Promotions.Conditions
{
    /// <summary>
    /// A SiteCore Commerce condition for the qualification
    /// "Order history contains products in the [specific category] for a total [compares] [specific value]"
    /// </summary>
    [EntityIdentifier("Promethium_" + nameof(OrderHistoryTotalInCategoryCondition))]
    public class OrderHistoryTotalInCategoryCondition : ICondition, ICustomerCondition
    {
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;

        public OrderHistoryTotalInCategoryCondition(FindEntitiesInListCommand findEntitiesInListCommand)
        {
            _findEntitiesInListCommand = findEntitiesInListCommand;
        }

        public IRuleValue<string> SpecificCategory { get; set; }

        public IBinaryOperator<decimal, decimal> Compares { get; set; }

        public IRuleValue<decimal> SpecificValue { get; set; }

        public IRuleValue<bool> IncludeSubCategories { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            //Get configuration
            var specificCategory = SpecificCategory.Yield(context);
            var specificValue = SpecificValue.Yield(context);
            var includeSubCategories = IncludeSubCategories.Yield(context);
            if (string.IsNullOrEmpty(specificCategory) || specificValue == 0 || Compares == null)
            {
                return false;
            }

            //Get data
            if (!context.GetOrderHistory(_findEntitiesInListCommand, specificCategory, includeSubCategories, out var categoryLines))
            {
                return false;
            }

            //Validate data against configuration
            var categoryTotal = categoryLines.Sum(line => line.Totals.GrandTotal.Amount);
            return Compares.Evaluate(categoryTotal, specificValue);
        }
    }
}
