using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Sample;
using Sitecore.Commerce.Plugin.Catalog;

namespace Plugin.Promotions.Pipelines.Blocks
{
    public class AddCategoryBlock : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        public override Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            var cartLine = context.CommerceContext.GetObject<CartLineArgument>();
            var addedLine = arg.Lines.FirstOrDefault(line => line.Id.Equals(cartLine.Line.Id, StringComparison.OrdinalIgnoreCase));

            var sellableItem = context.CommerceContext.GetEntity<SellableItem>();

            var categoryComponent = addedLine.GetComponent<CategoryComponent>();
            categoryComponent.ParentCategoryList = sellableItem.ParentCategoryList;
            return Task.FromResult(arg);
        }
    }
}







