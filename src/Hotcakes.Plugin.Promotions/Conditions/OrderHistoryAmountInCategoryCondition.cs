using System.Linq;

using Hotcakes.Plugin.Promotions.Resolvers;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Rules;

namespace Hotcakes.Plugin.Promotions.Conditions
{
    /// <summary>
    /// A Sitecore Commerce condition for the qualification
    /// "Order history contains [compares] [specific value] products in the [specific] category"
    /// </summary>
    [EntityIdentifier("Hc_" + nameof(OrderHistoryAmountInCategoryCondition))]
    public class OrderHistoryAmountInCategoryCondition : ICustomerCondition
    {
        private readonly CategoryOrderLinesResolver categoryOrderLinesResolver;

        public OrderHistoryAmountInCategoryCondition(CategoryOrderLinesResolver categoryOrderLinesResolver)
        {
            this.categoryOrderLinesResolver = categoryOrderLinesResolver;
        }

        public IBinaryOperator<decimal, decimal> Hc_Compares { get; set; }

        public IRuleValue<decimal> Hc_SpecificValue { get; set; }

        public IRuleValue<string> Hc_SpecificCategory { get; set; }

        public IRuleValue<bool> Hc_IncludeSubCategories { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            //Get configuration
            var specificCategory = Hc_SpecificCategory.Yield(context);
            var specificValue = Hc_SpecificValue.Yield(context);
            var includeSubCategories = Hc_IncludeSubCategories.Yield(context);
            if (string.IsNullOrEmpty(specificCategory) || specificValue == 0 || Hc_Compares == null)
            {
                return false;
            }

            //Get Data
            var commerceContext = context.Fact<CommerceContext>();
            var categoryLines = AsyncHelper.RunSync(() =>
                categoryOrderLinesResolver.Resolve(commerceContext, specificCategory, includeSubCategories));
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
