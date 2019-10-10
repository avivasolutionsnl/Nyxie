using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Framework.Rules;
using System;
using System.Linq;

namespace Promethium.Plugin.Promotions.Actions
{
    /// <summary>
    /// A SiteCore Commerce action for the benefit
    /// "For every [Items to award] of [Items to purchase] products in [Category] you get [Percentage Off] on the [Apply Award To] with a limit of [Award Limit]"
    /// </summary>
    [EntityIdentifier("Promethium_" + nameof(CartEveryXItemsInCategoryPercentageDiscountAction))]
    public class CartEveryXItemsInCategoryPercentageDiscountAction : ICartLineAction
    {
        public IRuleValue<int> Promethium_ItemsToAward { get; set; }

        public IRuleValue<int> Promethium_ItemsToPurchase { get; set; }

        public IRuleValue<string> Promethium_SpecificCategory { get; set; }

        public IRuleValue<bool> Promethium_IncludeSubCategories { get; set; }

        public IRuleValue<decimal> Promethium_PercentageOff { get; set; }

        public IRuleValue<string> Promethium_ApplyActionTo { get; set; }

        public IRuleValue<int> Promethium_ActionLimit { get; set; }

        public void Execute(IRuleExecutionContext context)
        {
            var commerceContext = context.Fact<CommerceContext>();

            //Get configuration
            var specificCategory = Promethium_SpecificCategory.Yield(context);
            var itemsToAward = Promethium_ItemsToAward.Yield(context);
            var itemsToPurchase = Promethium_ItemsToPurchase.Yield(context);
            var includeSubCategories = Promethium_IncludeSubCategories.Yield(context);
            var percentageOff = Promethium_PercentageOff.Yield(context);
            var applyActionTo = Promethium_ApplyActionTo.Yield(context);
            var actionLimit = Promethium_ActionLimit.Yield(context);

            if (string.IsNullOrEmpty(specificCategory) ||
                itemsToAward == 0 ||
                itemsToPurchase == 0 ||
                percentageOff == 0 ||
                string.IsNullOrEmpty(applyActionTo) ||
                actionLimit == 0)
            {
                return;
            }

            //Get data
            if (!context.GetCardLines(specificCategory, includeSubCategories, out var categoryLines))
            {
                return;
            }

            //Validate and apply action
            var productAmount = Convert.ToInt32(categoryLines.Sum(x => x.Quantity));
            var productsToAward = (productAmount / itemsToPurchase) * itemsToAward;
            productsToAward = productsToAward > actionLimit ? actionLimit : productsToAward;
            if (productsToAward > 0)
            {
                categoryLines.ApplyAction(commerceContext, percentageOff, applyActionTo, productsToAward, nameof(CartEveryXItemsInCategoryPercentageDiscountAction), ActionExtensions.CalculatePercentageDiscount);
            }
        }
    }
}
