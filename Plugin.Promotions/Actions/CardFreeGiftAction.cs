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

        public CardFreeGiftAction(GetSellableItemCommand getCommand)
        {
            _getCommand = getCommand;
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

            //TODO Correct method to get the product?
            var sellableItem = _getCommand.Process(commerceContext, targetItemId, true).Result;
            if (sellableItem != null)
            {
                //TODO Check if this is correct implementation
                var freeGift = new CartLineComponent
                {
                    Id = sellableItem.Id,
                    Name = sellableItem.Name,
                    Quantity = quantity,
                    ItemId = sellableItem.ProductId,
                    UnitListPrice = sellableItem.ListPrice,
                };
                cart.Lines.Add(freeGift);

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