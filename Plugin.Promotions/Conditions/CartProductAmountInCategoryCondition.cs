using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Framework.Rules;
using System.Linq;

namespace Promethium.Plugin.Promotions.Conditions
{
    /// <summary>
    /// A SiteCore Commerce condition for the qualification
    /// "Cart contains [compares] [specific value] products in the [specific category]"
    /// </summary>
    [EntityIdentifier("Promethium_" + nameof(CartProductAmountInCategoryCondition))]
    public class CartProductAmountInCategoryCondition : ICondition, ICartsCondition
    {
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
            if (!context.GetCardLines(specificCategory, includeSubCategories, out var categoryLines))
            {
                return false;
            }

            //Validate data against configuration
            var productAmount = categoryLines.Count();
            return Compares.Evaluate(productAmount, specificValue);
        }
    }
}
