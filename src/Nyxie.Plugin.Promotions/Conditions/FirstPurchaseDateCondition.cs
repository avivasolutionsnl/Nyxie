using System;
using System.Collections.Generic;
using System.Linq;

using Nyxie.Plugin.Promotions.Resolvers;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Framework.Rules;

namespace Nyxie.Plugin.Promotions.Conditions
{
    /// <summary>
    ///     A Sitecore Commerce condition for the qualification
    ///     "Current Customer first purchase is [operator] [date]"
    /// </summary>
    [EntityIdentifier("Hc_" + nameof(FirstPurchaseDateCondition))]
    public class FirstPurchaseDateCondition : ICustomerCondition
    {
        private readonly OrderResolver orderResolver;

        public FirstPurchaseDateCondition(OrderResolver orderResolver)
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

            DateTime firstPurchaseDate = orders.Min(x => x.OrderPlacedDate).DateTime;
            return Hc_Operator.Evaluate(firstPurchaseDate, date);
        }
    }
}
