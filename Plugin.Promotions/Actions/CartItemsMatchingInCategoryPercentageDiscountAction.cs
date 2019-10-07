using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Framework.Rules;
using System.Linq;

namespace Promethium.Plugin.Promotions.Actions
{
    /// <summary>
    /// A SiteCore Commerce action for the benefit
    /// "When you buy [Operator] [specific value] products in [specific category] you get [Percentage off] per product (ordered by [apply award to]) with a maximum of [award limit] products"
    /// </summary>
    [EntityIdentifier("Promethium_" + nameof(CartItemsMatchingInCategoryPercentageDiscountAction))]
    public class CartItemsMatchingInCategoryPercentageDiscountAction : ICartLineAction
    {
        public IBinaryOperator<decimal, decimal> Promethium_Operator { get; set; }

        public IRuleValue<decimal> Promethium_SpecificValue { get; set; }

        public IRuleValue<string> Promethium_SpecificCategory { get; set; }

        public IRuleValue<bool> Promethium_IncludeSubCategories { get; set; }

        public IRuleValue<decimal> Promethium_PercentageOff { get; set; }

        public IRuleValue<string> Promethium_ApplyActionTo { get; set; }

        public IRuleValue<decimal> Promethium_AwardLimit { get; set; }

        public void Execute(IRuleExecutionContext context)
        {
            var commerceContext = context.Fact<CommerceContext>();

            //Get configuration
            var specificCategory = Promethium_SpecificCategory.Yield(context);
            var specificValue = Promethium_SpecificValue.Yield(context);
            var includeSubCategories = Promethium_IncludeSubCategories.Yield(context);
            var percentageOff = Promethium_PercentageOff.Yield(context);
            var applyActionTo = Promethium_ApplyActionTo.Yield(context);
            var awardLimit = Promethium_AwardLimit.Yield(context);

            if (string.IsNullOrEmpty(specificCategory) ||
                specificValue == 0 ||
                percentageOff == 0 ||
                string.IsNullOrEmpty(applyActionTo) ||
                awardLimit == 0 ||
                Promethium_Operator == null)
            {
                return;
            }

            //Get data
            if (!context.GetCardLines(specificCategory, includeSubCategories, out var categoryLines))
            {
                return;
            }

            var productAmount = categoryLines.Sum(x => x.Quantity);
            if (Promethium_Operator.Evaluate(productAmount, specificValue))
            {
                categoryLines.ApplyAction(commerceContext, percentageOff, applyActionTo, awardLimit, nameof(CartItemsMatchingInCategoryPercentageDiscountAction), ActionExtensions.CalculatePercentageDiscount);
            }
        }
    }
}
