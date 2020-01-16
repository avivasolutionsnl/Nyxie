using System.Linq;
using System.Threading.Tasks;

using Nyxie.Plugin.Promotions.Components;
using Nyxie.Plugin.Promotions.Extensions;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Management;
using Sitecore.Framework.Pipelines;
using Sitecore.Services.Core.Model;

namespace Nyxie.Plugin.Promotions.Pipelines.Blocks
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
            CartLineComponent addedLine = arg.Lines.FirstOrDefault(line => line.Id.EqualsOrdinalIgnoreCase(cartLine.Line.Id));
            if (addedLine == null)
                return arg;

            SellableItem sellableItem = await getSellableItemCommand.Process(_commerceContext, addedLine.ItemId, false);

            var categoryComponent = addedLine.GetComponent<CategoryComponent>();
            if (sellableItem.ParentCategoryList != null && !categoryComponent.ParentCategoryList.Any())
                foreach (string categoryId in sellableItem.ParentCategoryList.Split('|'))
                    categoryComponent.ParentCategoryList.Add(await GetCategoryIdPath(categoryId, ""));

            return arg;
        }

        private async Task<string> GetCategoryIdPath(string categoryId, string input)
        {
            //Place parent path before the current children output
            string output = $"/{categoryId}{input}";

            ItemModel item = await _manager.GetItemByIdAsync(_commerceContext, categoryId);

            object parentCategoryList = item["ParentCategoryList"];

            if (parentCategoryList == null)
                return output;

            string parentCategoryId = parentCategoryList.ToString();

            if (string.IsNullOrEmpty(parentCategoryId))
                return output;

            return await GetCategoryIdPath(parentCategoryId, output);
        }
    }
}
