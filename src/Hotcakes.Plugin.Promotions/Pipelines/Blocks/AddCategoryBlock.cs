using System.Linq;
using System.Threading.Tasks;

using Hotcakes.Plugin.Promotions.Components;
using Hotcakes.Plugin.Promotions.Extensions;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Management;
using Sitecore.Framework.Pipelines;

namespace Hotcakes.Plugin.Promotions.Pipelines.Blocks
{
    public class AddCategoryBlock : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        private readonly SitecoreConnectionManager _manager;
        private readonly GetSellableItemCommand getSellableItemCommand;
        private CommerceContext _commerceContext;

        public AddCategoryBlock(SitecoreConnectionManager manager, GetSellableItemCommand getSellableItemCommand)
        {
            _manager = manager;
            this.getSellableItemCommand = getSellableItemCommand;
        }

        public override async Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            _commerceContext = context.CommerceContext;
            
            var cartLine = context.CommerceContext.GetObject<CartLineArgument>();
            var addedLine = arg.Lines.FirstOrDefault(line => line.Id.EqualsOrdinalIgnoreCase(cartLine.Line.Id));
            if (addedLine == null)
            {
                return arg;
            }

            var sellableItem = await getSellableItemCommand.Process(_commerceContext, addedLine.ItemId, false);

            var categoryComponent = addedLine.GetComponent<CategoryComponent>();
            if (sellableItem.ParentCategoryList != null && !categoryComponent.ParentCategoryList.Any())
            {
                foreach (var categoryId in sellableItem.ParentCategoryList.Split('|'))
                {
                    categoryComponent.ParentCategoryList.Add(await GetCategoryIdPath(categoryId, ""));
                }
            }

            return arg;
        }

        private async Task<string> GetCategoryIdPath(string categoryId, string input)
        {
            //Place parent path before the current children output
            var output = $"/{categoryId}{input}";

            var item = await _manager.GetItemByIdAsync(_commerceContext, categoryId);

            var parentCategoryList = item["ParentCategoryList"];

            if (parentCategoryList == null)
            {
                return output;
            }

            var parentCategoryId = parentCategoryList.ToString();

            if (string.IsNullOrEmpty(parentCategoryId))
            {
                return output;
            }

            return await GetCategoryIdPath(parentCategoryId, output);
        }
    }
}