using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Orders;

namespace Hotcakes.Plugin.Promotions.Resolvers
{
    public class OrderResolver
    {
        private readonly FindEntitiesInListCommand findEntitiesInListCommand;

        public OrderResolver(FindEntitiesInListCommand findEntitiesInListCommand)
        {
            this.findEntitiesInListCommand = findEntitiesInListCommand;
        }

        public async Task<List<Order>> Resolve(CommerceContext commerceContext)
        {
            if (commerceContext == null || !commerceContext.CurrentUserIsRegistered())
                return null;

            string listName = string.Format(CultureInfo.InvariantCulture,
                commerceContext.GetPolicy<KnownOrderListsPolicy>().CustomerOrders,
                commerceContext.CurrentCustomerId());

            CommerceList<Order> result =
                await findEntitiesInListCommand.Process<Order>(commerceContext, listName, 0, int.MaxValue);

            return result?.Items;
        }
    }
}
