using Promethium.Plugin.Promotions.Components;
using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Management;
using Sitecore.Framework.Pipelines;
using System.Linq;
using System.Threading.Tasks;

namespace Promethium.Plugin.Promotions.Pipelines.Blocks
{
    public class AddCategoryBlock : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        private CommerceContext _commerceContext;

        public override async Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            _commerceContext = context.CommerceContext;

            var sellableItem = context.CommerceContext.GetEntity<SellableItem>();

            var cartLine = context.CommerceContext.GetObject<CartLineArgument>();
            var addedLine = arg.Lines.FirstOrDefault(line => line.Id.EqualsOrdinalIgnoreCase(cartLine.Line.Id));
            if (addedLine == null)
            {
                return arg;
            }

            var categoryComponent = addedLine.GetComponent<CategoryComponent>();
            if (sellableItem.ParentCategoryList != null && !categoryComponent.ParentCategoryList.Any())
            {
                var manager = new SitecoreConnectionManager();

                foreach (var categoryId in sellableItem.ParentCategoryList.Split('|'))
                {
                    categoryComponent.ParentCategoryList.Add(await GetCategoryPath(categoryId, "", manager));
                }
            }

            return arg;
        }

        private async Task<string> GetCategoryPath(string categoryId, string input, SitecoreConnectionManager manager)
        {
            //Place parent path before the current children output
            var output = $"/{categoryId}{input}";

            var item = await manager.GetItemByIdAsync(_commerceContext, input);
            //var category = _categories.FirstOrDefault(x => x.SitecoreId.Equals(categoryId));

            return item["ParentCategoryList"] != null ?
                await GetCategoryPath(item["ParentCategoryList"].ToString(), output, manager) :
                output;
        }
    }
}