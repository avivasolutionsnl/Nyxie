using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Rules;
using System.Linq;
using Promethium.Plugin.Promotions.Resolvers;

namespace Promethium.Plugin.Promotions.Actions
{
    /// <summary>
    /// A Sitecore Commerce action for the benefit
    /// "When you buy [Operator] [Product count] products in [Category] you get [Amount off] per product (ordered by [apply award to]) with a maximum of [award limit] products"
    /// </summary>
    [EntityIdentifier("Pm_" + nameof(CartItemsMatchingInCategoryPriceDiscountAction))]
    public class CartItemsMatchingInCategoryPriceDiscountAction : ICartLineAction
    {
        private readonly CategoryCartLinesResolver categoryCartLinesResolver;

        public CartItemsMatchingInCategoryPriceDiscountAction(CategoryCartLinesResolver categoryCartLinesResolver)
        {
            this.categoryCartLinesResolver = categoryCartLinesResolver;
        }

        public IBinaryOperator<decimal, decimal> Pm_Operator { get; set; }

        public IRuleValue<decimal> Pm_SpecificValue { get; set; }

        public IRuleValue<string> Pm_SpecificCategory { get; set; }

        public IRuleValue<bool> Pm_IncludeSubCategories { get; set; }

        public IRuleValue<decimal> Pm_AmountOff { get; set; }

        public IRuleValue<string> Pm_ApplyActionTo { get; set; }

        public IRuleValue<int> Pm_ActionLimit { get; set; }

        public void Execute(IRuleExecutionContext context)
        {
            var commerceContext = context.Fact<CommerceContext>();

            //Get configuration
            var specificCategory = Pm_SpecificCategory.Yield(context);
            var specificValue = Pm_SpecificValue.Yield(context);
            var includeSubCategories = Pm_IncludeSubCategories.Yield(context);
            var amountOff = Pm_AmountOff.Yield(context);
            var applyActionTo = Pm_ApplyActionTo.Yield(context);
            var actionLimit = Pm_ActionLimit.Yield(context);

            if (string.IsNullOrEmpty(specificCategory) ||
                specificValue == 0 ||
                amountOff == 0 ||
                string.IsNullOrEmpty(applyActionTo) ||
                actionLimit == 0 ||
                Pm_Operator == null)
            {
                return;
            }

            //Get data
            var categoryLines = categoryCartLinesResolver.Resolve(commerceContext, specificCategory, includeSubCategories);
            if (categoryLines == null)
            {
                return;
            }
            
            //Validate and apply action
            var productAmount = categoryLines.Sum(x => x.Quantity);
            if (!Pm_Operator.Evaluate(productAmount, specificValue))
            {
                return;
            }

            var discountApplicator = new DiscountApplicator(commerceContext);
            discountApplicator.ApplyPriceDiscount(categoryLines, amountOff, new DiscountOptions
            {
                ActionLimit = actionLimit,
                ApplicationOrder = ApplicationOrder.Parse(applyActionTo),
                AwardingBlock = nameof(CartItemsMatchingInCategoryPriceDiscountAction)
            });
        }
    }
}
