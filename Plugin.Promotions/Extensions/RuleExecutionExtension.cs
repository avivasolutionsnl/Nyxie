using Promethium.Plugin.Promotions.Components;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
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
        internal static async Task<List<CartLineComponent>> GetCardLines(this IRuleExecutionContext context,
            string specificCategory,
            bool includeSubCategories,
            GetCategoryCommand getCommand)
        {
            var commerceContext = context.Fact<CommerceContext>();
            var cart = commerceContext.GetObject<Cart>();
            if (cart == null || !cart.Lines.Any())
            {
                return null;
            }

            var category = await getCommand.Process(commerceContext, specificCategory);

            return cart.Lines.Where(line => line.GetComponent<CategoryComponent>().IsMatch(category.SitecoreId, includeSubCategories)).ToList();
        }

        internal static async Task<List<Order>> GetOrderHistory(this IRuleExecutionContext context, FindEntitiesInListCommand findEntitiesInListCommand)
        {
            var commerceContext = context.Fact<CommerceContext>();
            if (commerceContext == null || !commerceContext.CurrentUserIsRegistered())
            {
                return null;
            }

            var listName = string.Format(CultureInfo.InvariantCulture,
                commerceContext.GetPolicy<KnownOrderListsPolicy>().CustomerOrders,
                commerceContext.CurrentCustomerId());
            var result = await findEntitiesInListCommand.Process<Order>(commerceContext, listName, 0, int.MaxValue);
            return result?.Items;
        }

        internal static async Task<List<CartLineComponent>> GetOrderHistory(this IRuleExecutionContext context,
            FindEntitiesInListCommand findEntitiesInListCommand,
            string specificCategory,
            bool includeSubCategories,
            GetCategoryCommand getCommand)
        {
            var commerceContext = context.Fact<CommerceContext>();
            var foundOrders = await context.GetOrderHistory(findEntitiesInListCommand);

            var category = await getCommand.Process(commerceContext, specificCategory);

            return foundOrders?
                .SelectMany(x => x.Lines)
                .Where(line => line.HasComponent<CategoryComponent>() && line.GetComponent<CategoryComponent>().IsMatch(category.SitecoreId, includeSubCategories))
                .ToList();
        }
    }
}
