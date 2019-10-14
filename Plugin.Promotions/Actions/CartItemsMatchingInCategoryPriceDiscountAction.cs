using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Framework.Rules;
using System.Linq;

namespace Promethium.Plugin.Promotions.Actions
{
    /// <summary>
    /// A SiteCore Commerce action for the benefit
    /// "When you buy [Operator] [Product count] products in [Category] you get [Amount off] per product (ordered by [apply award to]) with a maximum of [award limit] products"
    /// </summary>
    [EntityIdentifier("Pm_" + nameof(CartItemsMatchingInCategoryPriceDiscountAction))]
    public class CartItemsMatchingInCategoryPriceDiscountAction : ICartLineAction
    {
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
            if (!context.GetCardLines(specificCategory, includeSubCategories, out var categoryLines))
            {
                return;
            }

            //Validate and apply action
            var productAmount = categoryLines.Sum(x => x.Quantity);
            if (Pm_Operator.Evaluate(productAmount, specificValue))
            {
                categoryLines.ApplyAction(commerceContext, amountOff, applyActionTo, actionLimit, nameof(CartItemsMatchingInCategoryPriceDiscountAction), ActionExtensions.CalculatePriceDiscount);
            }
        }
    }
}
