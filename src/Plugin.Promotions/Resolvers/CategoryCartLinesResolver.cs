using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Promethium.Plugin.Promotions.Components;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;

namespace Promethium.Plugin.Promotions.Resolvers
{
    public class CategoryCartLinesResolver
    {
        private readonly GetCategoryCommand getCategoryCommand;

        public CategoryCartLinesResolver(GetCategoryCommand getCategoryCommand)
        {
            this.getCategoryCommand = getCategoryCommand;
        }

        public IEnumerable<CartLineComponent> Resolve(CommerceContext commerceContext, string category, bool includeSubCategories)
        {
            //Get data
            var categorySitecoreId = AsyncHelper.RunSync(() => GetSitecoreIdFromCommerceId(commerceContext, category));

            return GetLinesMatchingCategory(commerceContext, categorySitecoreId, includeSubCategories);
        }

        private List<CartLineComponent> GetLinesMatchingCategory(CommerceContext commerceContext, string categorySitecoreId, bool includeSubCategories)
        {
            var lines = GetCartLines(commerceContext);
            return lines.Where(line => line.GetComponent<CategoryComponent>().IsMatch(categorySitecoreId, includeSubCategories)).ToList();
        }

        private IEnumerable<CartLineComponent> GetCartLines(CommerceContext commerceContext)
        {
            var cart = commerceContext.GetObject<Cart>();
            if (cart == null || !cart.Lines.Any())
            {
                return null;
            }

            return cart.Lines;
        }

        private async Task<string> GetSitecoreIdFromCommerceId(CommerceContext commerceContext, string categoryCommerceId)
        {
            var category = await getCategoryCommand.Process(commerceContext, categoryCommerceId);
            
            return category?.SitecoreId;
        }
    }
}
