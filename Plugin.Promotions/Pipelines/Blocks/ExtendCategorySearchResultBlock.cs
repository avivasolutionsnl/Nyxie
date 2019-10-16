using Promethium.Plugin.Promotions.Extensions;
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
        private CommerceContext _commerceContext;

        public ExtendCategorySearchResultBlock(GetCategoryCommand getCommand)
        {
            _getCommand = getCommand;
        }

        public override async Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            _commerceContext = context.CommerceContext;
            var manager = new SitecoreConnectionManager();

            var results = arg.ChildViews.OfType<EntityView>().Where(x => x.ItemId.IndexOf("-Category", StringComparison.OrdinalIgnoreCase) > 0);
            foreach (var result in results)
            {
                var categoryIdentifier = result.ItemId;
                var category = await _getCommand.Process(_commerceContext, categoryIdentifier);

                if (category != null)
                {
                    var categoryPath = "";
                    var parentId = category.ParentCategoryList;

                    while (true)
                    {
                        var parent = await manager.GetItemByIdAsync(_commerceContext, parentId);
                        if (parent == null || parent["ParentCategoryList"] == null ||
                            string.IsNullOrWhiteSpace(parent["ParentCategoryList"].ToString()))
                        {
                            break;
                        }

                        categoryPath = $"/{parent["DisplayName"]}{categoryPath}";

                        parentId = parent["ParentCategoryList"].ToString();
                    }

                    var displayName = category.DisplayName;
                    if(categoryPath.Length > 0)
                    {
                        displayName += $" in {categoryPath}";
                    }

                    var displayProperty = result.Properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("DisplayName"));
                    if (displayProperty != null)
                    {
                        displayProperty.Value = displayName;
                    }
                }
            }

            return arg;
        }
    }
}
