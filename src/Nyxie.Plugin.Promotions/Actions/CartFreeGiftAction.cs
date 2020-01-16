using System.Linq;

using Nyxie.Plugin.Promotions.Extensions;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Rules;

namespace Nyxie.Plugin.Promotions.Actions
{
    /// <summary>
    ///     A Sitecore Commerce action for the benefit
    ///     "Get [quantity] free [gift]"
    /// </summary>
    [EntityIdentifier("Hc_" + nameof(CartFreeGiftAction))]
    public class CartFreeGiftAction : ICartAction
    {
        private readonly AddCartLineCommand _addCommand;
        private readonly GetSellableItemCommand _getCommand;

        public CartFreeGiftAction(GetSellableItemCommand getCommand, AddCartLineCommand addCommand)
        {
            _getCommand = getCommand;
            _addCommand = addCommand;
        }

        public IRuleValue<decimal> Hc_Quantity { get; set; }

        public IRuleValue<string> TargetItemId { get; set; }

        public void Execute(IRuleExecutionContext context)
        {
            var commerceContext = context.Fact<CommerceContext>();
            var cart = commerceContext?.GetObject<Cart>();
            if (cart == null || !cart.Lines.Any())
                return;

            decimal quantity = Hc_Quantity.Yield(context);
            string targetItemId = TargetItemId.Yield(context);

            if (quantity == 0 || string.IsNullOrEmpty(targetItemId))
                return;

            if (cart.Lines.Any(x => x.ItemId == targetItemId && x.Quantity == quantity))
                //After the product is added
                //You only get the product for free once :)
                return;

            cart.GetComponent<MessagesComponent>().AddPromotionApplied(commerceContext, nameof(CartFreeGiftAction));

            CartLineComponent giftLine = CreateLine(cart, commerceContext, targetItemId, quantity);

            if (giftLine == null)
                return;

            cart.Lines.Add(giftLine);
        }

        private CartLineComponent CreateLine(Cart cart, CommerceContext commerceContext, string targetItemId, decimal quantity)
        {
            SellableItem gift = AsyncHelper.RunSync(() => _getCommand.Process(commerceContext, targetItemId, false));
            if (gift == null)
                return null;

            // To make sure all pipeline blocks are executed and do not influence the current cart, we add
            // the gift line to a temporary cart and then copy it to the current cart. 
            var temporaryCart = cart.Clone<Cart>();
            temporaryCart.AddComponents(new TemporaryCartComponent(cart.Id));

            var freeGift = new CartLineComponent
            {
                ItemId = targetItemId,
                Quantity = quantity
            };

            temporaryCart = AsyncHelper.RunSync(() => _addCommand.Process(commerceContext, temporaryCart, freeGift));

            CartLineComponent line = temporaryCart.Lines.Single(x => x.ItemId == targetItemId);

            if (gift.ListPrice.Amount > 0)
            {
                decimal discount = new MoneyEx(commerceContext, gift.ListPrice).Round().Value.Amount;

                line.Adjustments.Add(AwardedAdjustmentFactory.CreateLineLevelAwardedAdjustment(discount * -1,
                    nameof(CartFreeGiftAction), line.Id, commerceContext));
            }

            return line;
        }
    }
}
