using System;

using Sitecore.Commerce.Plugin.Orders;

namespace Promethium.Plugin.Promotions.Tests.Builders
{
    public class OrderBuilder
    {
        private DateTimeOffset orderPlacedDate;

        public OrderBuilder PlacedOn(DateTimeOffset orderPlacedDate)
        {
            this.orderPlacedDate = orderPlacedDate;
            return this;
        }

        public Order Build()
        {
            return new Order
            {
                OrderPlacedDate = orderPlacedDate
            };
        }
    }
}
