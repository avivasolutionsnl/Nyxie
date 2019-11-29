using Promethium.Plugin.Promotions.Components;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Orders;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Promethium.Plugin.Promotions.Factory
{
    internal class OrderHistoryFactory
    {
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;
        private readonly CommerceContext _commerceContext;

        public OrderHistoryFactory(CommerceContext commerceContext, FindEntitiesInListCommand findEntitiesInListCommand)
        {
            _commerceContext = commerceContext;
            _findEntitiesInListCommand = findEntitiesInListCommand;
        }

        internal async Task<List<Order>> GetAllOrders()
        {
            if (_commerceContext == null || !_commerceContext.CurrentUserIsRegistered())
            {
                return null;
            }

            var listName = string.Format(CultureInfo.InvariantCulture,
                _commerceContext.GetPolicy<KnownOrderListsPolicy>().CustomerOrders,
                _commerceContext.CurrentCustomerId());
            var result = await _findEntitiesInListCommand.Process<Order>(_commerceContext, listName, 0, int.MaxValue);
            return result?.Items;
        }

        internal async Task<List<CartLineComponent>> GetOrderHistory(string categorySitecoreId, bool includeSubCategories)
        {
            var allOrders = await GetAllOrders();

            return allOrders?
                .SelectMany(x => x.Lines)
                .Where(line => line.HasComponent<CategoryComponent>() && line.GetComponent<CategoryComponent>().IsMatch(categorySitecoreId, includeSubCategories))
                .ToList();
        }
    }
}
