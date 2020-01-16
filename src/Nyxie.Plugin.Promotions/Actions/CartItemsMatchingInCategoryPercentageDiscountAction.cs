using System.Collections.Generic;
using System.Linq;

using Nyxie.Plugin.Promotions.Resolvers;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Framework.Rules;

namespace Nyxie.Plugin.Promotions.Actions
{
    /// <summary>
    ///     A Sitecore Commerce action for the benefit
    ///     "When you buy [Operator] [specific value] products in [specific category] you get [Percentage off] per product
    ///     (ordered by [apply award to]) with a maximum of [award limit] products"
    /// </summary>
    [EntityIdentifier("Hc_" + nameof(CartItemsMatchingInCategoryPercentageDiscountAction))]
    public class CartItemsMatchingInCategoryPercentageDiscountAction : ICartLineAction
    {
        private readonly CategoryCartLinesResolver categoryCartLinesResolver;

        public CartItemsMatchingInCategoryPercentageDiscountAction(CategoryCartLinesResolver categoryCartLinesResolver)
        {
            this.categoryCartLinesResolver = categoryCartLinesResolver;
        }

        public IBinaryOperator<decimal, decimal> Hc_Operator { get; set; }

        public IRuleValue<decimal> Hc_SpecificValue { get; set; }

        public IRuleValue<string> Hc_SpecificCategory { get; set; }

        public IRuleValue<bool> Hc_IncludeSubCategories { get; set; }

        public IRuleValue<decimal> Hc_PercentageOff { get; set; }

        public IRuleValue<string> Hc_ApplyActionTo { get; set; }

        public IRuleValue<int> Hc_ActionLimit { get; set; }

        public void Execute(IRuleExecutionContext context)
        {
            var commerceContext = context.Fact<CommerceContext>();

            //Get configuration
            string specificCategory = Hc_SpecificCategory.Yield(context);
            decimal specificValue = Hc_SpecificValue.Yield(context);
            bool includeSubCategories = Hc_IncludeSubCategories.Yield(context);
            decimal percentageOff = Hc_PercentageOff.Yield(context);
            string applyActionTo = Hc_ApplyActionTo.Yield(context);
            int actionLimit = Hc_ActionLimit.Yield(context);

            if (string.IsNullOrEmpty(specificCategory) ||
                specificValue == 0 ||
                percentageOff == 0 ||
                string.IsNullOrEmpty(applyActionTo) ||
                actionLimit == 0 ||
                Hc_Operator == null)
                return;

            //Get data
            IEnumerable<CartLineComponent> categoryLines =
                categoryCartLinesResolver.Resolve(commerceContext, specificCategory, includeSubCategories);
            if (categoryLines == null)
                return;

            //Validate and apply action
            decimal productAmount = categoryLines.Sum(x => x.Quantity);
            if (!Hc_Operator.Evaluate(productAmount, specificValue))
                return;

            var discountApplicator = new DiscountApplicator(commerceContext);
            discountApplicator.ApplyPercentageDiscount(categoryLines, percentageOff, new DiscountOptions
            {
                ActionLimit = actionLimit,
                ApplicationOrder = ApplicationOrder.Parse(applyActionTo),
                AwardingBlock = nameof(CartItemsMatchingInCategoryPercentageDiscountAction)
            });
        }
    }
}
