using Promethium.Plugin.Promotions.Components;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Framework.Rules;
using System.Linq;

namespace Promethium.Plugin.Promotions.Conditions
{
    [EntityIdentifier("IsProductFromCategory")]
    public class IsProductFromCategory : ICondition
    {
        public IRuleValue<string> Category { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            string category = this.Category.Yield(context);
            Cart cart = context.Fact<CommerceContext>()?.GetObject<Cart>();
            if (cart == null || !cart.Lines.Any() || string.IsNullOrEmpty(category))
            {
                return false;
            }
            return cart.Lines.Any(line => line.GetComponent<CategoryComponent>().ParentCategoryList.Contains(category));            
        }
    }
}
