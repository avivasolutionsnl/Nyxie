using System;
using System.Linq;

using Hotcakes.Plugin.Promotions.Resolvers;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Framework.Rules;

namespace Hotcakes.Plugin.Promotions.Actions
{
    /// <summary>
    /// A Sitecore Commerce action for the benefit
    /// "For every [Items to award] of [Items to purchase] products in [Category] you get [Percentage Off] on the [Apply Award To] with a limit of [Award Limit]"
    /// </summary>
    [EntityIdentifier("Hc_" + nameof(CartEveryXItemsInCategoryPercentageDiscountAction))]
    public class CartEveryXItemsInCategoryPercentageDiscountAction : ICartLineAction
    {
        private readonly CategoryCartLinesResolver categoryCartLinesResolver;

        public CartEveryXItemsInCategoryPercentageDiscountAction(CategoryCartLinesResolver categoryCartLinesResolver)
        {
            this.categoryCartLinesResolver = categoryCartLinesResolver;
        }

        public IRuleValue<decimal> Hc_ItemsToAward { get; set; }

        public IRuleValue<decimal> Hc_ItemsToPurchase { get; set; }

        public IRuleValue<string> Hc_SpecificCategory { get; set; }

        public IRuleValue<bool> Hc_IncludeSubCategories { get; set; }

        public IRuleValue<decimal> Hc_PercentageOff { get; set; }

        public IRuleValue<string> Hc_ApplyActionTo { get; set; }

        public IRuleValue<int> Hc_ActionLimit { get; set; }

        public void Execute(IRuleExecutionContext context)
        {
            var commerceContext = context.Fact<CommerceContext>();

            //Get configuration
            var specificCategory = Hc_SpecificCategory.Yield(context);
            var itemsToAward = Hc_ItemsToAward.Yield(context);
            var itemsToPurchase = Hc_ItemsToPurchase.Yield(context);
            var includeSubCategories = Hc_IncludeSubCategories.Yield(context);
            var percentageOff = Hc_PercentageOff.Yield(context);
            var applyActionTo = Hc_ApplyActionTo.Yield(context);
            var actionLimit = Hc_ActionLimit.Yield(context);

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
            var categoryLines = categoryCartLinesResolver.Resolve(commerceContext, specificCategory, includeSubCategories);
            if (categoryLines == null)
            {
                return;
            }

            //Validate and apply action
            var cartQuantity = Convert.ToInt32(categoryLines.Sum(x => x.Quantity));
            var cartProductsToAward = (cartQuantity / itemsToPurchase) * itemsToAward;

            var productsToAward = cartProductsToAward > actionLimit ? actionLimit : cartProductsToAward;

            if (productsToAward <= 0)
            {
                return;
            }

            var discountApplicator = new DiscountApplicator(commerceContext);

            discountApplicator.ApplyPercentageDiscount(categoryLines, percentageOff, new DiscountOptions
            {
                ActionLimit = Convert.ToInt32(productsToAward),
                ApplicationOrder = ApplicationOrder.Parse(applyActionTo),
                AwardingBlock = nameof(CartEveryXItemsInCategoryPercentageDiscountAction)
            });
        }
    }
}
