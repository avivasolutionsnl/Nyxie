using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Framework.Rules;
using System.Linq;

namespace Promethium.Plugin.Promotions.Conditions
{
    /// <summary>
    /// A SiteCore Commerce condition for the qualification
    /// "Cart contains products in the [specific category] for a total [compares] [specific value]"
    /// </summary>
    [EntityIdentifier("Promethium_" + nameof(CartProductTotalInCategoryCondition))]
    public class CartProductTotalInCategoryCondition : ICartsCondition
    {
        public IRuleValue<string> Promethium_SpecificCategory { get; set; }

        public IBinaryOperator<decimal, decimal> Promethium_Compares { get; set; }

        public IRuleValue<decimal> Promethium_SpecificValue { get; set; }

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

            //Get data
            if (!context.GetCardLines(specificCategory, includeSubCategories, out var categoryLines))
            {
                return false;
            }

            //Validate data against configuration
            var categoryTotal = categoryLines.Sum(line => line.Totals.GrandTotal.Amount);
            return Promethium_Compares.Evaluate(categoryTotal, specificValue);
        }
    }
}
