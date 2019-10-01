using Promethium.Plugin.Promotions.Components;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Promethium.Plugin.Promotions.Pipelines.Blocks
{
    public class AddCategoryBlock : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        public override Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            var sellableItem = context.CommerceContext.GetEntity<SellableItem>();

            var cartLine = context.CommerceContext.GetObject<CartLineArgument>();
            var addedLine = arg.Lines.FirstOrDefault(line => line.Id.Equals(cartLine.Line.Id, StringComparison.OrdinalIgnoreCase));
            if (addedLine == null) return Task.FromResult(arg);

            var categoryComponent = addedLine.GetComponent<CategoryComponent>();
            categoryComponent.ParentCategoryList = sellableItem.ParentCategoryList;

            return Task.FromResult(arg);
        }
    }
}