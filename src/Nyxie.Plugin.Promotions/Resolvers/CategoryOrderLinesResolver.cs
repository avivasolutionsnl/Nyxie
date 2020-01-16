using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Nyxie.Plugin.Promotions.Components;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Orders;

namespace Nyxie.Plugin.Promotions.Resolvers
{
    public class CategoryOrderLinesResolver
    {
        private readonly GetCategoryCommand getCategoryCommand;
        private readonly OrderResolver orderResolver;

        public CategoryOrderLinesResolver(GetCategoryCommand getCategoryCommand, OrderResolver orderResolver)
        {
            this.getCategoryCommand = getCategoryCommand;
            this.orderResolver = orderResolver;
        }

        internal async Task<List<CartLineComponent>> Resolve(CommerceContext commerceContext, string category,
            bool includeSubCategories)
        {
            List<Order> allOrders = await orderResolver.Resolve(commerceContext);

            string categorySitecoreId = AsyncHelper.RunSync(() => GetSitecoreIdFromCommerceId(commerceContext, category));

            return allOrders?.SelectMany(x => x.Lines)
                            .Where(line => HasCategory(includeSubCategories, line, categorySitecoreId))
                            .ToList();
        }

        private static bool HasCategory(bool includeSubCategories, CartLineComponent line, string categorySitecoreId)
        {
            return line.HasComponent<CategoryComponent>() &&
                   line.GetComponent<CategoryComponent>().IsMatch(categorySitecoreId, includeSubCategories);
        }

        private async Task<string> GetSitecoreIdFromCommerceId(CommerceContext commerceContext, string categoryCommerceId)
        {
            Category category = await getCategoryCommand.Process(commerceContext, categoryCommerceId);

            return category?.SitecoreId;
        }
    }
}
