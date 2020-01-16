using System.Collections.Generic;
using System.Linq;

using Nyxie.Plugin.Promotions.Resolvers;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Rules;

namespace Nyxie.Plugin.Promotions.Conditions
{
    /// <summary>
    ///     A Sitecore Commerce condition for the qualification
    ///     "Order history contains [compares] [specific value] products in the [specific] category"
    /// </summary>
    [EntityIdentifier("Ny_" + nameof(OrderHistoryAmountInCategoryCondition))]
    public class OrderHistoryAmountInCategoryCondition : ICustomerCondition
    {
        private readonly CategoryOrderLinesResolver categoryOrderLinesResolver;

        public OrderHistoryAmountInCategoryCondition(CategoryOrderLinesResolver categoryOrderLinesResolver)
        {
            this.categoryOrderLinesResolver = categoryOrderLinesResolver;
        }

        public IBinaryOperator<decimal, decimal> Ny_Compares { get; set; }

        public IRuleValue<decimal> Ny_SpecificValue { get; set; }

        public IRuleValue<string> Ny_SpecificCategory { get; set; }

        public IRuleValue<bool> Ny_IncludeSubCategories { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            //Get configuration
            string specificCategory = Ny_SpecificCategory.Yield(context);
            decimal specificValue = Ny_SpecificValue.Yield(context);
            bool includeSubCategories = Ny_IncludeSubCategories.Yield(context);
            if (string.IsNullOrEmpty(specificCategory) || specificValue == 0 || Ny_Compares == null)
                return false;

            //Get Data
            var commerceContext = context.Fact<CommerceContext>();
            List<CartLineComponent> categoryLines = AsyncHelper.RunSync(() =>
                categoryOrderLinesResolver.Resolve(commerceContext, specificCategory, includeSubCategories));
            if (categoryLines == null)
                return false;

            //Validate data against configuration
            decimal productAmount = categoryLines.Sum(x => x.Quantity);
            return Ny_Compares.Evaluate(productAmount, specificValue);
        }
    }
}
