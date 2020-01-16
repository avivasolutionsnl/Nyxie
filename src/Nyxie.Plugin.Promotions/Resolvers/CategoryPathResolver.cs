using System.Threading.Tasks;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Management;
using Sitecore.Services.Core.Model;

namespace Nyxie.Plugin.Promotions.Resolvers
{
    public class CategoryPathResolver
    {
        private readonly GetCategoryCommand getCategoryCommand;
        private readonly SitecoreConnectionManager manager;

        public CategoryPathResolver(SitecoreConnectionManager manager, GetCategoryCommand getCategoryCommand)
        {
            this.manager = manager;
            this.getCategoryCommand = getCategoryCommand;
        }

        public async Task<string> GetCategoryPath(CommerceContext commerceContext, string categoryCommerceId)
        {
            Category category = await getCategoryCommand.Process(commerceContext, categoryCommerceId);
            if (category == null)
                return categoryCommerceId;

            string parentPath = await GetParentPath(commerceContext, category.ParentCategoryList, string.Empty);

            return $"{parentPath}/{category.DisplayName}";
        }

        public async Task<string> GetParentPath(CommerceContext commerceContext, string parentSitecoreId, string input)
        {
            ItemModel parent = await manager.GetItemByIdAsync(commerceContext, parentSitecoreId);
            if (parent?["ParentCategoryList"] == null || string.IsNullOrWhiteSpace(parent["ParentCategoryList"].ToString()))
                return input;

            string output = $"/{parent["DisplayName"]}{input}";
            return await GetParentPath(commerceContext, parent["ParentCategoryList"].ToString(), output);
        }
    }
}
