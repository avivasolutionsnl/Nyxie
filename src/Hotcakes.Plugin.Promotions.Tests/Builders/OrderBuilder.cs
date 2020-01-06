using System;
using System.Linq;

using Sitecore.Commerce.Plugin.Orders;

namespace Hotcakes.Plugin.Promotions.Tests.Builders
{
    public class OrderBuilder
    {
        private LineBuilder[] lineBuilders = { new LineBuilder() };
        private DateTimeOffset orderPlacedDate;

        public OrderBuilder PlacedOn(DateTimeOffset orderPlacedDate)
        {
            this.orderPlacedDate = orderPlacedDate;
            return this;
        }

        public OrderBuilder WithLines(params LineBuilder[] lineBuilders)
        {
            this.lineBuilders = lineBuilders;
            return this;
        }

        public Order Build()
        {
            return new Order
            {
                OrderPlacedDate = orderPlacedDate,
                Lines = lineBuilders.Select(x => x.Build()).ToList()
            };
        }
    }
}
