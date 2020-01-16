using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Nyxie.Plugin.Promotions.Extensions;
using Nyxie.Plugin.Promotions.Resolvers;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;

namespace Nyxie.Plugin.Promotions.Pipelines.Blocks
{
    public class ExtendCategorySearchResultBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly GetCategoryCommand _getCommand;
        private readonly CategoryPathResolver categoryPathResolver;

        public ExtendCategorySearchResultBlock(GetCategoryCommand getCommand, CategoryPathResolver categoryPathResolver)
        {
            _getCommand = getCommand;
            this.categoryPathResolver = categoryPathResolver;
        }

        public override async Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            CommerceContext commerceContext = context.CommerceContext;

            IEnumerable<EntityView> results = arg.ChildViews.OfType<EntityView>()
                                                 .Where(
                                                     x => x.ItemId.IndexOf("-Category", StringComparison.OrdinalIgnoreCase) > 0);
            foreach (EntityView result in results)
            {
                ViewProperty displayProperty =
                    result.Properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("DisplayName"));
                if (displayProperty == null)
                    continue;

                Category category = await _getCommand.Process(commerceContext, result.ItemId);
                if (category == null)
                    continue;

                string parentPath =
                    await categoryPathResolver.GetParentPath(commerceContext, category.ParentCategoryList,
                        string.Empty);

                if (parentPath.Length > 0)
                    displayProperty.Value = $"{category.DisplayName} in {parentPath}";
                else
                    displayProperty.Value = category.DisplayName;
            }

            return arg;
        }
    }
}
