using System.Collections.Generic;
using System.Linq;

using Nyxie.Plugin.Promotions.Resolvers;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Framework.Rules;

namespace Nyxie.Plugin.Promotions.Conditions
{
    /// <summary>
    ///     A Sitecore Commerce condition for the qualification
    ///     "Cart contains products in the [specific category] for a total [compares] [specific value]"
    /// </summary>
    [EntityIdentifier("Ny_" + nameof(CartProductTotalInCategoryCondition))]
    public class CartProductTotalInCategoryCondition : ICartsCondition
    {
        private readonly CategoryCartLinesResolver categoryCartLinesResolver;

        public CartProductTotalInCategoryCondition(CategoryCartLinesResolver categoryCartLinesResolver)
        {
            this.categoryCartLinesResolver = categoryCartLinesResolver;
        }

        public IRuleValue<string> Ny_SpecificCategory { get; set; }

        public IBinaryOperator<decimal, decimal> Ny_Compares { get; set; }

        public IRuleValue<decimal> Ny_SpecificValue { get; set; }

        public IRuleValue<bool> Ny_IncludeSubCategories { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            var commerceContext = context.Fact<CommerceContext>();

            //Get configuration
            string specificCategory = Ny_SpecificCategory.Yield(context);
            decimal specificValue = Ny_SpecificValue.Yield(context);
            bool includeSubCategories = Ny_IncludeSubCategories.Yield(context);
            if (string.IsNullOrEmpty(specificCategory) || specificValue == 0 || Ny_Compares == null)
                return false;

            //Get data
            IEnumerable<CartLineComponent> categoryLines =
                categoryCartLinesResolver.Resolve(commerceContext, specificCategory, includeSubCategories);
            if (categoryLines == null)
                return false;

            //Validate data against configuration
            decimal categoryTotal = categoryLines.Sum(line => line.Totals.GrandTotal.Amount);
            return Ny_Compares.Evaluate(categoryTotal, specificValue);
        }
    }
}
