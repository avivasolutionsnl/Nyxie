using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Framework.Rules;
using System;
using System.Linq;

namespace Promethium.Plugin.Promotions.Actions
{
    /// <summary>
    /// A Sitecore Commerce action for the benefit
    /// "For every [Items to award] of [Items to purchase] products in [Category] you get [Percentage Off] on the [Apply Award To] with a limit of [Award Limit]"
    /// </summary>
    [EntityIdentifier("Pm_" + nameof(CartEveryXItemsInCategoryPercentageDiscountAction))]
    public class CartEveryXItemsInCategoryPercentageDiscountAction : ICartLineAction
    {
        public IRuleValue<decimal> Pm_ItemsToAward { get; set; }

        public IRuleValue<decimal> Pm_ItemsToPurchase { get; set; }

        public IRuleValue<string> Pm_SpecificCategory { get; set; }

        public IRuleValue<bool> Pm_IncludeSubCategories { get; set; }

        public IRuleValue<decimal> Pm_PercentageOff { get; set; }

        public IRuleValue<string> Pm_ApplyActionTo { get; set; }

        public IRuleValue<int> Pm_ActionLimit { get; set; }

        public void Execute(IRuleExecutionContext context)
        {
            var commerceContext = context.Fact<CommerceContext>();

            //Get configuration
            var specificCategory = Pm_SpecificCategory.Yield(context);
            var itemsToAward = Pm_ItemsToAward.Yield(context);
            var itemsToPurchase = Pm_ItemsToPurchase.Yield(context);
            var includeSubCategories = Pm_IncludeSubCategories.Yield(context);
            var percentageOff = Pm_PercentageOff.Yield(context);
            var applyActionTo = Pm_ApplyActionTo.Yield(context);
            var actionLimit = Pm_ActionLimit.Yield(context);

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
            var categoryLines = context.GetCardLines(specificCategory, includeSubCategories);
            if (categoryLines == null)
            {
                return;
            }

            //Validate and apply action
            var cartQuantity = Convert.ToInt32(categoryLines.Sum(x => x.Quantity));
            var cartProductsToAward = (cartQuantity / itemsToPurchase) * itemsToAward;

            var productsToAward = cartProductsToAward > actionLimit ? actionLimit : cartProductsToAward;
            
            if (productsToAward > 0)
            {
                commerceContext.ApplyAction(categoryLines, percentageOff, applyActionTo, Convert.ToInt32(productsToAward), nameof(CartEveryXItemsInCategoryPercentageDiscountAction), CommerceContextExtensions.CalculatePercentageDiscount);
            }
        }
    }
}
