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
    [EntityIdentifier("Promethium_" + nameof(CartItemsMatchingInCategoryPriceDiscountAction))]
    public class CartItemsMatchingInCategoryPriceDiscountAction : ICartLineAction
    {
        public IBinaryOperator<decimal, decimal> Promethium_Operator { get; set; }

        public IRuleValue<decimal> Promethium_SpecificValue { get; set; }

        public IRuleValue<string> Promethium_SpecificCategory { get; set; }

        public IRuleValue<bool> Promethium_IncludeSubCategories { get; set; }

        public IRuleValue<decimal> Promethium_AmountOff { get; set; }

        public IRuleValue<string> Promethium_ApplyActionTo { get; set; }

        public IRuleValue<int> Promethium_ActionLimit { get; set; }

        public void Execute(IRuleExecutionContext context)
        {
            var commerceContext = context.Fact<CommerceContext>();

            //Get configuration
            var specificCategory = Promethium_SpecificCategory.Yield(context);
            var specificValue = Promethium_SpecificValue.Yield(context);
            var includeSubCategories = Promethium_IncludeSubCategories.Yield(context);
            var amountOff = Promethium_AmountOff.Yield(context);
            var applyActionTo = Promethium_ApplyActionTo.Yield(context);
            var actionLimit = Promethium_ActionLimit.Yield(context);

            if (string.IsNullOrEmpty(specificCategory) ||
                specificValue == 0 ||
                amountOff == 0 ||
                string.IsNullOrEmpty(applyActionTo) ||
                actionLimit == 0 ||
                Promethium_Operator == null)
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
            if (Promethium_Operator.Evaluate(productAmount, specificValue))
            {
                categoryLines.ApplyAction(commerceContext, amountOff, applyActionTo, actionLimit, nameof(CartItemsMatchingInCategoryPriceDiscountAction), ActionExtensions.CalculatePriceDiscount);
            }
        }
    }
}
