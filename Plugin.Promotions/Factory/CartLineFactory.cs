using Promethium.Plugin.Promotions.Components;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using System.Collections.Generic;
using System.Linq;

namespace Promethium.Plugin.Promotions.Factory
{
    internal class CartLineFactory
    {
        private readonly CommerceContext _commerceContext;

        internal CartLineFactory(CommerceContext commerceContext)
        {
            _commerceContext = commerceContext;
        }

        internal List<CartLineComponent> GetLinesMatchingCategory(
            string categorySitecoreId,
            bool includeSubCategories)
        {
            var lines = GetCartLines();
            return lines.Where(line => line.GetComponent<CategoryComponent>().IsMatch(categorySitecoreId, includeSubCategories)).ToList();
        }

        private IEnumerable<CartLineComponent> GetCartLines()
        {
            var cart = _commerceContext.GetObject<Cart>();
            if (cart == null || !cart.Lines.Any())
            {
                return null;
            }

            return cart.Lines;
        }
    }
}
