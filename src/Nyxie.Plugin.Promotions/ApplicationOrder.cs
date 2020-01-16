using System;
using System.Collections.Generic;
using System.Linq;

using Nyxie.Plugin.Promotions.Properties;

using Sitecore.Commerce.Plugin.Carts;

namespace Nyxie.Plugin.Promotions
{
    public class ApplicationOrder
    {
        private readonly Func<IEnumerable<CartLineComponent>, IEnumerable<CartLineComponent>> order;

        private ApplicationOrder(string name, string displayName,
            Func<IEnumerable<CartLineComponent>, IEnumerable<CartLineComponent>> order)
        {
            this.order = order;
            Name = name;
            DisplayName = displayName;
        }

        public string Name { get; }

        public string DisplayName { get; }

        public static ApplicationOrder Ascending =>
            new ApplicationOrder("Price.Ascending", Resources.PriceAscending_DisplayName,
                lines => lines.OrderBy(x => x.UnitListPrice.Amount));

        public static ApplicationOrder Descending =>
            new ApplicationOrder("Price.Descending", Resources.PriceDescending_DisplayName,
                lines => lines.OrderByDescending(x => x.UnitListPrice.Amount));

        public static IEnumerable<ApplicationOrder> All => new[] { Ascending, Descending };

        public IEnumerable<CartLineComponent> Order(IEnumerable<CartLineComponent> lines)
        {
            return order(lines);
        }

        public static ApplicationOrder Parse(string name)
        {
            return All.Single(x => x.Name == name);
        }
    }
}
