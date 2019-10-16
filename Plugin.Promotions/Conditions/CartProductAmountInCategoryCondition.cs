using Promethium.Plugin.Promotions.Classes;
using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Rules;
using System.Linq;

namespace Promethium.Plugin.Promotions.Conditions
{
    /// <summary>
    /// A Sitecore Commerce condition for the qualification
    /// "Cart contains [compares] [specific value] products in the [specific category]"
    /// </summary>
    [EntityIdentifier("Pm_" + nameof(CartProductAmountInCategoryCondition))]
    public class CartProductAmountInCategoryCondition : ICartsCondition
    {
        private readonly GetCategoryCommand _getCategoryCommand;

        public CartProductAmountInCategoryCondition(GetCategoryCommand getCategoryCommand)
        {
            _getCategoryCommand = getCategoryCommand;
        }

        public IBinaryOperator<decimal, decimal> Pm_Compares { get; set; }

        public IRuleValue<decimal> Pm_SpecificValue { get; set; }

        public IRuleValue<string> Pm_SpecificCategory { get; set; }

        public IRuleValue<bool> Pm_IncludeSubCategories { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            //Get configuration
            var specificCategory = Pm_SpecificCategory.Yield(context);
            var specificValue = Pm_SpecificValue.Yield(context);
            var includeSubCategories = Pm_IncludeSubCategories.Yield(context);
            if (string.IsNullOrEmpty(specificCategory) || specificValue == 0 || Pm_Compares == null)
            {
                return false;
            }

            //Get Data
            var categoryLines = AsyncHelper.RunSync(() => context.GetCardLines(specificCategory, includeSubCategories, _getCategoryCommand));
            if (categoryLines == null)
            {
                return false;
            }

            //Validate data against configuration
            var productAmount = categoryLines.Sum(x => x.Quantity);
            return Pm_Compares.Evaluate(productAmount, specificValue);
        }
    }
}
