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
    ///     "When you buy [Operator] [Product count] products in [Category] you get [Amount off] per product (ordered by [apply
    ///     award to]) with a maximum of [award limit] products"
    /// </summary>
    [EntityIdentifier("Ny_" + nameof(CartItemsMatchingInCategoryPriceDiscountAction))]
    public class CartItemsMatchingInCategoryPriceDiscountAction : ICartLineAction
    {
        private readonly CategoryCartLinesResolver categoryCartLinesResolver;

        public CartItemsMatchingInCategoryPriceDiscountAction(CategoryCartLinesResolver categoryCartLinesResolver)
        {
            this.categoryCartLinesResolver = categoryCartLinesResolver;
        }

        public IBinaryOperator<decimal, decimal> Ny_Operator { get; set; }

        public IRuleValue<decimal> Ny_SpecificValue { get; set; }

        public IRuleValue<string> Ny_SpecificCategory { get; set; }

        public IRuleValue<bool> Ny_IncludeSubCategories { get; set; }

        public IRuleValue<decimal> Ny_AmountOff { get; set; }

        public IRuleValue<string> Ny_ApplyActionTo { get; set; }

        public IRuleValue<int> Ny_ActionLimit { get; set; }

        public void Execute(IRuleExecutionContext context)
        {
            var commerceContext = context.Fact<CommerceContext>();

            //Get configuration
            string specificCategory = Ny_SpecificCategory.Yield(context);
            decimal specificValue = Ny_SpecificValue.Yield(context);
            bool includeSubCategories = Ny_IncludeSubCategories.Yield(context);
            decimal amountOff = Ny_AmountOff.Yield(context);
            string applyActionTo = Ny_ApplyActionTo.Yield(context);
            int actionLimit = Ny_ActionLimit.Yield(context);

            if (string.IsNullOrEmpty(specificCategory) ||
                specificValue == 0 ||
                amountOff == 0 ||
                string.IsNullOrEmpty(applyActionTo) ||
                actionLimit == 0 ||
                Ny_Operator == null)
                return;

            //Get data
            IEnumerable<CartLineComponent> categoryLines =
                categoryCartLinesResolver.Resolve(commerceContext, specificCategory, includeSubCategories);
            if (categoryLines == null)
                return;

            //Validate and apply action
            decimal productAmount = categoryLines.Sum(x => x.Quantity);
            if (!Ny_Operator.Evaluate(productAmount, specificValue))
                return;

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
