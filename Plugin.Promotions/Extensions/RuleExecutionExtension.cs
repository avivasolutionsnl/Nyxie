using Promethium.Plugin.Promotions.Components;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Framework.Rules;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Promethium.Plugin.Promotions.Extensions
{
    internal static class RuleExecutionExtension
    {
        internal static bool GetCardLines(this IRuleExecutionContext context,
            string specificCategory,
            bool includeSubCategories,
            out List<CartLineComponent> foundLines)
        {
            foundLines = null;

            var cart = context.Fact<CommerceContext>()?.GetObject<Cart>();
            if (cart == null || !cart.Lines.Any())
            {
                return false;
            }

            foundLines = cart.Lines.Where(line => line.GetComponent<CategoryComponent>().IsMatch(specificCategory, includeSubCategories)).ToList();
            return true;
        }

        internal static bool GetOrderHistory(this IRuleExecutionContext context, FindEntitiesInListCommand findEntitiesInListCommand, out List<Order> foundOrders)
        {
            foundOrders = null;

            var commerceContext = context.Fact<CommerceContext>();
            if (commerceContext == null || !commerceContext.CurrentUserIsRegistered())
            { return false; }

            var listName = string.Format(CultureInfo.InvariantCulture,
                commerceContext.GetPolicy<KnownOrderListsPolicy>().CustomerOrders,
                commerceContext.CurrentCustomerId());
            var task = Task.Run(() => findEntitiesInListCommand.Process<Order>(commerceContext, listName, 0, int.MaxValue));

            foundOrders = task.Result?.Items.ToList();

            return true;
        }

        internal static bool GetOrderHistory(this IRuleExecutionContext context,
            FindEntitiesInListCommand findEntitiesInListCommand,
            string specificCategory,
            bool includeSubCategories,
            out List<CartLineComponent> foundOrderLines)
        {
            foundOrderLines = null;

            var valid = context.GetOrderHistory(findEntitiesInListCommand, out var foundOrders);
            if (!valid)
                return false;

            foundOrderLines = foundOrders.SelectMany(x => x.Lines).Where(line => line.GetComponent<CategoryComponent>().IsMatch(specificCategory, includeSubCategories)).ToList();

            return true;
        }
    }
}
