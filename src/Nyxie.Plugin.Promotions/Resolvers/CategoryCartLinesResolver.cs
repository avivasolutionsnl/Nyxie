using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Nyxie.Plugin.Promotions.Components;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;

namespace Nyxie.Plugin.Promotions.Resolvers
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
            string categorySitecoreId = AsyncHelper.RunSync(() => GetSitecoreIdFromCommerceId(commerceContext, category));

            return GetLinesMatchingCategory(commerceContext, categorySitecoreId, includeSubCategories);
        }

        private List<CartLineComponent> GetLinesMatchingCategory(CommerceContext commerceContext, string categorySitecoreId,
            bool includeSubCategories)
        {
            IEnumerable<CartLineComponent> lines = GetCartLines(commerceContext);

            if (lines == null)
                return new List<CartLineComponent>();

            return lines.Where(line => line.GetComponent<CategoryComponent>().IsMatch(categorySitecoreId, includeSubCategories))
                        .ToList();
        }

        private IEnumerable<CartLineComponent> GetCartLines(CommerceContext commerceContext)
        {
            var cart = commerceContext.GetObject<Cart>();
            if (cart == null || !cart.Lines.Any())
                return null;

            return cart.Lines;
        }

        private async Task<string> GetSitecoreIdFromCommerceId(CommerceContext commerceContext, string categoryCommerceId)
        {
            Category category = await getCategoryCommand.Process(commerceContext, categoryCommerceId);

            return category?.SitecoreId;
        }
    }
}
