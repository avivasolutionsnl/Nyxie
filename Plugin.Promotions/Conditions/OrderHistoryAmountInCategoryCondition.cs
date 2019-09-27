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
    public class OrderHistoryAmountInCategoryCondition : ICondition, ICustomerCondition
    {
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;

        public OrderHistoryAmountInCategoryCondition(FindEntitiesInListCommand findEntitiesInListCommand)
        {
            _findEntitiesInListCommand = findEntitiesInListCommand;
        }

        public IBinaryOperator<int, int> Compares { get; set; }

        public IRuleValue<int> SpecificValue { get; set; }

        public IRuleValue<string> SpecificCategory { get; set; }

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

            //Get Data
            if (!context.GetOrderHistory(_findEntitiesInListCommand, specificCategory, includeSubCategories, out var categoryLines))
            {
                return false;
            }

            //Validate data against configuration
            var productAmount = categoryLines.Count();
            return Compares.Evaluate(productAmount, specificValue);
        }
    }
}
