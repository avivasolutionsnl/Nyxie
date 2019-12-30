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
    /// "Current Customer last purchase is [operator] [date]"
    /// </summary>
    [EntityIdentifier("Pm_" + nameof(LastPurchaseDateCondition))]
    public class LastPurchaseDateCondition : ICustomerCondition
    {
        private readonly OrderResolver orderResolver;

        public LastPurchaseDateCondition(OrderResolver orderResolver)
        {
            this.orderResolver = orderResolver;
        }

        //Sitecore only adds Datetime operators out-of-the-box
        public IBinaryOperator<DateTime, DateTime> Pm_Operator { get; set; }

        //Out-of-the-box DatetimeOffset get's a nice editor and Datetime not
        public IRuleValue<DateTimeOffset> Pm_Date { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            var date = Pm_Date.Yield(context).DateTime;

            var orders = AsyncHelper.RunSync(() => orderResolver.Resolve(context.Fact<CommerceContext>()));

            if (orders == null)
            {
                return false;
            }

            var lastPurchaseDate = orders.Max(x => x.OrderPlacedDate).DateTime;
            return Pm_Operator.Evaluate(lastPurchaseDate, date);
        }
    }
}
