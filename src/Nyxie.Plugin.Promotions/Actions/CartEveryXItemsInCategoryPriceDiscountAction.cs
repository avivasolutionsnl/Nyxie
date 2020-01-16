using System;
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
    ///     "For every [Items to award] of [Items to purchase] products in [Category] you get [Amount Off] on the [Apply Award
    ///     To] with a limit of [Award Limit]"
    /// </summary>
    [EntityIdentifier("Hc_" + nameof(CartEveryXItemsInCategoryPriceDiscountAction))]
    public class CartEveryXItemsInCategoryPriceDiscountAction : ICartLineAction
    {
        private readonly CategoryCartLinesResolver categoryCartLinesResolver;

        public CartEveryXItemsInCategoryPriceDiscountAction(CategoryCartLinesResolver categoryCartLinesResolver)
        {
            this.categoryCartLinesResolver = categoryCartLinesResolver;
        }

        public IRuleValue<decimal> Hc_ItemsToAward { get; set; }

        public IRuleValue<decimal> Hc_ItemsToPurchase { get; set; }

        public IRuleValue<string> Hc_SpecificCategory { get; set; }

        public IRuleValue<bool> Hc_IncludeSubCategories { get; set; }

        public IRuleValue<decimal> Hc_AmountOff { get; set; }

        public IRuleValue<string> Hc_ApplyActionTo { get; set; }

        public IRuleValue<int> Hc_ActionLimit { get; set; }

        public void Execute(IRuleExecutionContext context)
        {
            var commerceContext = context.Fact<CommerceContext>();

            //Get configuration
            string specificCategory = Hc_SpecificCategory.Yield(context);
            decimal itemsToAward = Hc_ItemsToAward.Yield(context);
            decimal itemsToPurchase = Hc_ItemsToPurchase.Yield(context);
            bool includeSubCategories = Hc_IncludeSubCategories.Yield(context);
            decimal amountOff = Hc_AmountOff.Yield(context);
            string applyActionTo = Hc_ApplyActionTo.Yield(context);
            int actionLimit = Hc_ActionLimit.Yield(context);

            if (string.IsNullOrEmpty(specificCategory) ||
                itemsToAward == 0 ||
                itemsToPurchase == 0 ||
                amountOff == 0 ||
                string.IsNullOrEmpty(applyActionTo) ||
                actionLimit == 0)
                return;

            //Get data
            IEnumerable<CartLineComponent> categoryLines =
                categoryCartLinesResolver.Resolve(commerceContext, specificCategory, includeSubCategories);
            if (categoryLines == null)
                return;

            //Validate and apply action
            int cartQuantity = Convert.ToInt32(categoryLines.Sum(x => x.Quantity));
            decimal cartProductsToAward = Math.Floor(cartQuantity / itemsToPurchase) * itemsToAward;

            decimal productsToAward = cartProductsToAward > actionLimit ? actionLimit : cartProductsToAward;

            if (productsToAward <= 0)
                return;

            var discountApplicator = new DiscountApplicator(commerceContext);
            discountApplicator.ApplyPriceDiscount(categoryLines, amountOff, new DiscountOptions
            {
                ActionLimit = Convert.ToInt32(productsToAward),
                ApplicationOrder = ApplicationOrder.Parse(applyActionTo),
                AwardingBlock = nameof(CartEveryXItemsInCategoryPriceDiscountAction)
            });
        }
    }
}
