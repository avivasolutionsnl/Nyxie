using System.Linq;

using Hotcakes.Plugin.Promotions.Resolvers;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Framework.Rules;

namespace Hotcakes.Plugin.Promotions.Conditions
{
    /// <summary>
    /// A Sitecore Commerce condition for the qualification
    /// "Cart contains products in the [specific category] for a total [compares] [specific value]"
    /// </summary>
    [EntityIdentifier("Hc_" + nameof(CartProductTotalInCategoryCondition))]
    public class CartProductTotalInCategoryCondition : ICartsCondition
    {
        private readonly CategoryCartLinesResolver categoryCartLinesResolver;

        public CartProductTotalInCategoryCondition(CategoryCartLinesResolver categoryCartLinesResolver)
        {
            this.categoryCartLinesResolver = categoryCartLinesResolver;
        }

        public IRuleValue<string> Hc_SpecificCategory { get; set; }

        public IBinaryOperator<decimal, decimal> Hc_Compares { get; set; }

        public IRuleValue<decimal> Hc_SpecificValue { get; set; }

        public IRuleValue<bool> Hc_IncludeSubCategories { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            var commerceContext = context.Fact<CommerceContext>();

            //Get configuration
            var specificCategory = Hc_SpecificCategory.Yield(context);
            var specificValue = Hc_SpecificValue.Yield(context);
            var includeSubCategories = Hc_IncludeSubCategories.Yield(context);
            if (string.IsNullOrEmpty(specificCategory) || specificValue == 0 || Hc_Compares == null)
            {
                return false;
            }

            //Get data
            var categoryLines = categoryCartLinesResolver.Resolve(commerceContext, specificCategory, includeSubCategories);
            if (categoryLines == null)
            {
                return false;
            }

            //Validate data against configuration
            var categoryTotal = categoryLines.Sum(line => line.Totals.GrandTotal.Amount);
            return Hc_Compares.Evaluate(categoryTotal, specificValue);
        }
    }
}
