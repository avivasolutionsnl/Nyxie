using System;
using System.Linq;

using Sitecore.Commerce.Plugin.Orders;

namespace Promethium.Plugin.Promotions.Tests.Builders
{
    public class OrderBuilder
    {
        private DateTimeOffset orderPlacedDate;
        private LineBuilder[] lineBuilders = new[] { new LineBuilder() };

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
