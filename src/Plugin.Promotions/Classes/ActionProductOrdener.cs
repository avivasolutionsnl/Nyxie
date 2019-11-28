using Promethium.Plugin.Promotions.Properties;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using System.Collections.Generic;
using System.Linq;

namespace Promethium.Plugin.Promotions.Classes
{
    internal sealed class ActionProductOrdener
    {
        private const string PriceAscendingName = "Price.Ascending";
        private const string PriceDescendingName = "Price.Descending";

        static ActionProductOrdener()
        {
            Options = new List<Selection>
            {
                new Selection { DisplayName = Resources.PriceAscending_DisplayName, Name = PriceAscendingName },
                new Selection { DisplayName = Resources.PriceDescending_DisplayName, Name = PriceDescendingName },
            };
        }

        internal static List<Selection> Options { get; set; }

        internal static IEnumerable<CartLineComponent> Order(IEnumerable<CartLineComponent> lines, string ordener)
        {
            switch (ordener)
            {
                case PriceAscendingName:
                    return lines.OrderBy(x => x.UnitListPrice.Amount);
                case PriceDescendingName:
                    return lines.OrderByDescending(x => x.UnitListPrice.Amount);
                default:
                    return lines;
            }
        }
    }
}
