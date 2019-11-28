using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Management;
using System.Threading.Tasks;
using Sitecore.Commerce.Plugin.Catalog;

namespace Promethium.Plugin.Promotions.Factory
{
    internal class CategoryFactory
    {
        private readonly CommerceContext _commerceContext;
        private readonly SitecoreConnectionManager _manager;
        private readonly GetCategoryCommand _getCategoryCommand;

        internal CategoryFactory(CommerceContext commerceContext, SitecoreConnectionManager manager, GetCategoryCommand getCategoryCommand)
        {
            _commerceContext = commerceContext;
            _manager = manager;
            _getCategoryCommand = getCategoryCommand;
        }

        internal async Task<string> GetSitecoreIdFromCommerceId(string categoryCommerceId)
        {
            var category = await _getCategoryCommand.Process(_commerceContext, categoryCommerceId);
            return category?.SitecoreId;
        }

        internal async Task<string> GetCategoryPath(string categoryCommerceId)
        {
            var category = await _getCategoryCommand.Process(_commerceContext, categoryCommerceId);
            if (category == null)
            {
                return categoryCommerceId;
            }

            var parentPath = await GetParentPath(category.ParentCategoryList, string.Empty);

            return $"{parentPath}/{category.DisplayName}";
        }

        internal async Task<string> GetParentPath(string parentSitecoreId, string input)
        {
            var parent = await _manager.GetItemByIdAsync(_commerceContext, parentSitecoreId);
            if (parent?["ParentCategoryList"] == null || string.IsNullOrWhiteSpace(parent["ParentCategoryList"].ToString()))
            {
                return input;
            }

            var output = $"/{parent["DisplayName"]}{input}";
            return await GetParentPath(parent["ParentCategoryList"].ToString(), output);
        }
    }
}
