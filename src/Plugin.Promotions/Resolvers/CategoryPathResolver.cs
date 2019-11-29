using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Management;

namespace Promethium.Plugin.Promotions.Resolvers
{
    public class CategoryPathResolver
    {
        private readonly SitecoreConnectionManager manager;
        private readonly GetCategoryCommand getCategoryCommand;

        internal CategoryPathResolver(SitecoreConnectionManager manager, GetCategoryCommand getCategoryCommand)
        {
            this.manager = manager;
            this.getCategoryCommand = getCategoryCommand;
        }

        public async Task<string> GetCategoryPath(CommerceContext commerceContext, string categoryCommerceId)
        {
            var category = await getCategoryCommand.Process(commerceContext, categoryCommerceId);
            if (category == null)
            {
                return categoryCommerceId;
            }

            var parentPath = await GetParentPath(commerceContext, category.ParentCategoryList, string.Empty);

            return $"{parentPath}/{category.DisplayName}";
        }

        public async Task<string> GetParentPath(CommerceContext commerceContext, string parentSitecoreId, string input)
        {
            var parent = await manager.GetItemByIdAsync(commerceContext, parentSitecoreId);
            if (parent?["ParentCategoryList"] == null || string.IsNullOrWhiteSpace(parent["ParentCategoryList"].ToString()))
            {
                return input;
            }

            var output = $"/{parent["DisplayName"]}{input}";
            return await GetParentPath(commerceContext, parent["ParentCategoryList"].ToString(), output);
        }
    }
}
