using Promethium.Plugin.Promotions.Extensions;
using Promethium.Plugin.Promotions.Factory;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Management;
using Sitecore.Framework.Pipelines;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Promethium.Plugin.Promotions.Pipelines.Blocks
{
    [PipelineDisplayName("Search.Promethium.Block.ExtendCategorySearchResult")]
    public class ExtendCategorySearchResultBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly GetCategoryCommand _getCommand;
        private readonly SitecoreConnectionManager _manager;
        private CommerceContext _commerceContext;

        public ExtendCategorySearchResultBlock(GetCategoryCommand getCommand, SitecoreConnectionManager manager)
        {
            _getCommand = getCommand;
            _manager = manager;
        }

        public override async Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            _commerceContext = context.CommerceContext;
            
            var factory = new CategoryFactory(context.CommerceContext, _manager, null);
            var results = arg.ChildViews.OfType<EntityView>().Where(x => x.ItemId.IndexOf("-Category", StringComparison.OrdinalIgnoreCase) > 0);
            foreach (var result in results)
            {
                var displayProperty = result.Properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("DisplayName"));
                if (displayProperty == null)
                {
                    continue;
                }
                
                var category = await _getCommand.Process(_commerceContext, result.ItemId);
                if (category == null)
                {
                    continue;
                }

                var parentPath = await factory.GetParentPath(category.ParentCategoryList, string.Empty);
                if(parentPath.Length > 0)
                {
                    displayProperty.Value = $"{category.DisplayName} in {parentPath}";
                }
                else
                {
                    displayProperty.Value = category.DisplayName;
                }
            }

            return arg;
        }
    }
}
