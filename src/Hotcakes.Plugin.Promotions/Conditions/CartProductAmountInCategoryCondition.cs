using System.Linq;

using Hotcakes.Plugin.Promotions.Resolvers;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Framework.Rules;

namespace Hotcakes.Plugin.Promotions.Conditions
{
    /// <summary>
    /// A Sitecore Commerce condition for the qualification
    /// "Cart contains [compares] [specific value] products in the [specific category]"
    /// </summary>
    [EntityIdentifier("Hc_" + nameof(CartProductAmountInCategoryCondition))]
    public class CartProductAmountInCategoryCondition : ICartsCondition
    {
        private readonly CategoryCartLinesResolver categoryCartLinesResolver;

        public CartProductAmountInCategoryCondition(CategoryCartLinesResolver categoryCartLinesResolver)
        {
            this.categoryCartLinesResolver = categoryCartLinesResolver;
        }

        public IBinaryOperator<decimal, decimal> Hc_Compares { get; set; }

        public IRuleValue<decimal> Hc_SpecificValue { get; set; }

        public IRuleValue<string> Hc_SpecificCategory { get; set; }

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

            //Get Data
            var categoryLines = categoryCartLinesResolver.Resolve(commerceContext, specificCategory, includeSubCategories);
            if (categoryLines == null)
            {
                return false;
            }

            //Validate data against configuration
            var productAmount = categoryLines.Sum(x => x.Quantity);
            return Hc_Compares.Evaluate(productAmount, specificValue);
        }
    }
}
