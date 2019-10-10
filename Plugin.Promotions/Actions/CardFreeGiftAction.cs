using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Rules;
using System.Linq;

namespace Promethium.Plugin.Promotions.Actions
{
    /// <summary>
    /// A SiteCore Commerce action for the benefit
    /// "Get [quantity] free [gift]"
    /// </summary>
    [EntityIdentifier("Promethium_" + nameof(CardFreeGiftAction))]
    public class CardFreeGiftAction : ICartAction
    {
        private GetSellableItemCommand _getCommand;
        private AddCartLineCommand _addCommand;

        public CardFreeGiftAction(GetSellableItemCommand getCommand, AddCartLineCommand addCommand)
        {
            _getCommand = getCommand;
            _addCommand = addCommand;
        }

        public IRuleValue<decimal> Promethium_Quantity { get; set; }

        public IRuleValue<string> TargetItemId { get; set; }

        public void Execute(IRuleExecutionContext context)
        {
            var commerceContext = context.Fact<CommerceContext>();
            var cart = commerceContext?.GetObject<Cart>();
            if (cart == null || !cart.Lines.Any())
            {
                return;
            }

            var quantity = Promethium_Quantity.Yield(context);
            var targetItemId = TargetItemId.Yield(context);

            if (quantity == 0 || string.IsNullOrEmpty(targetItemId))
            {
                return;
            }

            var sellableItem = _getCommand.Process(commerceContext, targetItemId, false).Result;
            if (sellableItem != null)
            {
                var freeGift = new CartLineComponent {
                    ItemId = sellableItem.ProductId,
                    Quantity = quantity,
                    //Id = sellableItem.Id,
                    //Name = sellableItem.Name,
                    //UnitListPrice = sellableItem.ListPrice,
                };

                _addCommand.Process(commerceContext, cart, freeGift).ConfigureAwait(false);

                if (sellableItem.ListPrice.Amount > 0)
                {
                    var discount = sellableItem.ListPrice.Amount.ShouldRoundPriceCalc(commerceContext);
                    cart.Adjustments.AddCartLevelAwardedAdjustment(commerceContext, discount * -1, nameof(CardFreeGiftAction));
                }

                cart.GetComponent<MessagesComponent>().AddPromotionApplied(commerceContext, nameof(CardFreeGiftAction));
            }
        }
    }
}