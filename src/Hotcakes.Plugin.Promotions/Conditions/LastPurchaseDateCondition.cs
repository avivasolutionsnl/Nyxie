using System;
using System.Collections.Generic;
using System.Linq;

using Hotcakes.Plugin.Promotions.Resolvers;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Framework.Rules;

namespace Hotcakes.Plugin.Promotions.Conditions
{
    /// <summary>
    ///     A Sitecore Commerce condition for the qualification
    ///     "Current Customer last purchase is [operator] [date]"
    /// </summary>
    [EntityIdentifier("Hc_" + nameof(LastPurchaseDateCondition))]
    public class LastPurchaseDateCondition : ICustomerCondition
    {
        private readonly OrderResolver orderResolver;

        public LastPurchaseDateCondition(OrderResolver orderResolver)
        {
            this.orderResolver = orderResolver;
        }

        //Sitecore only adds Datetime operators out-of-the-box
        public IBinaryOperator<DateTime, DateTime> Hc_Operator { get; set; }

        //Out-of-the-box DatetimeOffset get's a nice editor and Datetime not
        public IRuleValue<DateTimeOffset> Hc_Date { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            DateTime date = Hc_Date.Yield(context).DateTime;

            List<Order> orders = AsyncHelper.RunSync(() => orderResolver.Resolve(context.Fact<CommerceContext>()));

            if (orders == null)
                return false;

            DateTime lastPurchaseDate = orders.Max(x => x.OrderPlacedDate).DateTime;
            return Hc_Operator.Evaluate(lastPurchaseDate, date);
        }
    }
}
