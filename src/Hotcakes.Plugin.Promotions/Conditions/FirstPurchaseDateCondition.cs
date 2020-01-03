using System;
using System.Linq;

using Hotcakes.Plugin.Promotions.Resolvers;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Rules;

namespace Hotcakes.Plugin.Promotions.Conditions
{
    /// <summary>
    /// A Sitecore Commerce condition for the qualification
    /// "Current Customer first purchase is [operator] [date]"
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
            var date = Hc_Date.Yield(context).DateTime;
            var orders = AsyncHelper.RunSync(() => orderResolver.Resolve(context.Fact<CommerceContext>()));

            if (orders == null)
            {
                return false;
            }

            var firstPurchaseDate = orders.Min(x => x.OrderPlacedDate).DateTime;
            return Hc_Operator.Evaluate(firstPurchaseDate, date);
        }
    }
}
