using Promethium.Plugin.Promotions.Components;
using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Promethium.Plugin.Promotions.Pipelines.Blocks
{
    public class AddCategoryBlock : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        private readonly GetCategoriesCommand _getCommand;
        private List<Category> _categories;

        public AddCategoryBlock(GetCategoriesCommand getCommand)
        {
            _getCommand = getCommand;
        }

        public override async Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
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
                var catalog = context.CommerceContext.GetEntity<Catalog>();
                var result = await _getCommand.Process(context.CommerceContext, catalog.Name);
                _categories = result.ToList();

                foreach (var categoryId in sellableItem.ParentCategoryList.Split('|'))
                {
                    categoryComponent.ParentCategoryList.Add(GetCategoryPath(categoryId, ""));
                }
            }

            return arg;
        }

        private string GetCategoryPath(string categoryId, string input)
        {
            //Place parent path before the current children output
            var output = $"/{categoryId}{input}";

            var category = _categories.FirstOrDefault(x => x.SitecoreId.Equals(categoryId));

            return category?.ParentCategoryList != null ?
                GetCategoryPath(category.ParentCategoryList, output) :
                output;
        }
    }
}