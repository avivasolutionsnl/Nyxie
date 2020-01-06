using System.Collections.Generic;
using System.Linq;

using Hotcakes.Plugin.Promotions.Resolvers;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Rules;

namespace Hotcakes.Plugin.Promotions.Conditions
{
    /// <summary>
    ///     A Sitecore Commerce condition for the qualification
    ///     "Order history contains products in the [specific category] for a total [compares] [specific value]"
    /// </summary>
    [EntityIdentifier("Hc_" + nameof(OrderHistoryTotalInCategoryCondition))]
    public class OrderHistoryTotalInCategoryCondition : ICustomerCondition
    {
        private readonly CategoryOrderLinesResolver categoryOrderLinesResolver;

        public OrderHistoryTotalInCategoryCondition(CategoryOrderLinesResolver categoryOrderLinesResolver)
        {
            this.categoryOrderLinesResolver = categoryOrderLinesResolver;
        }

        public IRuleValue<string> Hc_SpecificCategory { get; set; }

        public IBinaryOperator<decimal, decimal> Hc_Compares { get; set; }

        public IRuleValue<decimal> Hc_SpecificValue { get; set; }

        public IRuleValue<bool> Hc_IncludeSubCategories { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            //Get configuration
            string specificCategory = Hc_SpecificCategory.Yield(context);
            decimal specificValue = Hc_SpecificValue.Yield(context);
            bool includeSubCategories = Hc_IncludeSubCategories.Yield(context);
            if (string.IsNullOrEmpty(specificCategory) || specificValue == 0 || Hc_Compares == null)
                return false;

            //Get data
            var commerceContext = context.Fact<CommerceContext>();
            List<CartLineComponent> categoryLines = AsyncHelper.RunSync(() =>
                categoryOrderLinesResolver.Resolve(commerceContext, specificCategory, includeSubCategories));
            if (categoryLines == null)
                return false;

            //Validate data against configuration
            decimal categoryTotal = categoryLines.Sum(line => line.Totals.GrandTotal.Amount);
            return Hc_Compares.Evaluate(categoryTotal, specificValue);
        }
    }
}
