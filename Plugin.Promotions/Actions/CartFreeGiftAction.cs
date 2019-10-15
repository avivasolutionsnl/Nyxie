using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Rules;
using System.Linq;

namespace Promethium.Plugin.Promotions.Actions
{
    /// <summary>
    /// A Sitecore Commerce action for the benefit
    /// "Get [quantity] free [gift]"
    /// </summary>
    [EntityIdentifier("Pm_" + nameof(CartFreeGiftAction))]
    public class CartFreeGiftAction : ICartAction
    {
        private readonly GetSellableItemCommand _getCommand;
        private readonly AddCartLineCommand _addCommand;

        public CartFreeGiftAction(GetSellableItemCommand getCommand, AddCartLineCommand addCommand)
        {
            _getCommand = getCommand;
            _addCommand = addCommand;
        }

        public IRuleValue<decimal> Pm_Quantity { get; set; }

        public IRuleValue<string> TargetItemId { get; set; }

        public void Execute(IRuleExecutionContext context)
        {
            var commerceContext = context.Fact<CommerceContext>();
            var cart = commerceContext?.GetObject<Cart>();
            if (cart == null || !cart.Lines.Any())
            {
                return;
            }

            var quantity = Pm_Quantity.Yield(context);
            var targetItemId = TargetItemId.Yield(context);

            if (quantity == 0 || string.IsNullOrEmpty(targetItemId))
            {
                return;
            }

            if (cart.Lines.Any(x => x.ItemId == targetItemId && x.Quantity == quantity))
            {
                //After the product is added
                //You only get the product for free once :)
                return;
            }

            var sellableItem = _getCommand.Process(commerceContext, targetItemId, false).Result;
            if (sellableItem != null)
            {
                var freeGift = new CartLineComponent {
                    ItemId = targetItemId,
                    Quantity = quantity,
                };

                if (sellableItem.ListPrice.Amount > 0)
                {
                    var discount = commerceContext.ShouldRoundPriceCalc(sellableItem.ListPrice.Amount);
                    freeGift.Adjustments.AddLineLevelAwardedAdjustment(commerceContext, discount * -1, nameof(CartFreeGiftAction), freeGift.Id);
                }

                cart = _addCommand.Process(commerceContext, cart, freeGift).Result;

                commerceContext.AddPromotionApplied(cart.GetComponent<MessagesComponent>(), nameof(CartFreeGiftAction));
            }
        }
    }
}