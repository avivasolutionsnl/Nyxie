using Promethium.Plugin.Promotions.Classes;
using Promethium.Plugin.Promotions.Factory;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Rules;

using System;
using System.Linq;

namespace Promethium.Plugin.Promotions.Actions
{
    /// <summary>
    /// A Sitecore Commerce action for the benefit
    /// "For every [Items to award] of [Items to purchase] products in [Category] you get [Amount Off] on the [Apply Award To] with a limit of [Award Limit]"
    /// </summary>
    [EntityIdentifier("Pm_" + nameof(CartEveryXItemsInCategoryPriceDiscountAction))]
    public class CartEveryXItemsInCategoryPriceDiscountAction : ICartLineAction
    {
        private readonly GetCategoryCommand _getCategoryCommand;

        public CartEveryXItemsInCategoryPriceDiscountAction(GetCategoryCommand getCategoryCommand)
        {
            _getCategoryCommand = getCategoryCommand;
        }

        public IRuleValue<decimal> Pm_ItemsToAward { get; set; }

        public IRuleValue<decimal> Pm_ItemsToPurchase { get; set; }

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
            var itemsToAward = Pm_ItemsToAward.Yield(context);
            var itemsToPurchase = Pm_ItemsToPurchase.Yield(context);
            var includeSubCategories = Pm_IncludeSubCategories.Yield(context);
            var amountOff = Pm_AmountOff.Yield(context);
            var applyActionTo = Pm_ApplyActionTo.Yield(context);
            var actionLimit = Pm_ActionLimit.Yield(context);

            if (string.IsNullOrEmpty(specificCategory) ||
                itemsToAward == 0 ||
                itemsToPurchase == 0 ||
                amountOff == 0 ||
                string.IsNullOrEmpty(applyActionTo) ||
                actionLimit == 0)
            {
                return;
            }

            //Get data
            var categoryFactory = new CategoryFactory(commerceContext, null, _getCategoryCommand);
            var categorySitecoreId = AsyncHelper.RunSync(() => categoryFactory.GetSitecoreIdFromCommerceId(specificCategory));

            var cartLineFactory = new CartLineFactory(commerceContext);
            var categoryLines = cartLineFactory.GetLinesMatchingCategory(categorySitecoreId, includeSubCategories);
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
            discountApplicator.ApplyPriceDiscount(categoryLines, amountOff, new DiscountOptions
            {
                ActionLimit = Convert.ToInt32(productsToAward),
                ApplicationOrder = ApplicationOrder.Parse(applyActionTo),
                AwardingBlock = nameof(CartEveryXItemsInCategoryPriceDiscountAction)
            });
        }
    }
}
