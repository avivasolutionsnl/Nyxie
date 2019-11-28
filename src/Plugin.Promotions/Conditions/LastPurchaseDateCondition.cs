using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Rules;
using System;
using System.Linq;
using Promethium.Plugin.Promotions.Classes;

namespace Promethium.Plugin.Promotions.Conditions
{
    /// <summary>
    /// A Sitecore Commerce condition for the qualification
    /// "Current Customer last purchase is [operator] [date]"
    /// </summary>
    [EntityIdentifier("Pm_" + nameof(LastPurchaseDateCondition))]
    public class LastPurchaseDateCondition : ICustomerCondition
    {
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;

        public LastPurchaseDateCondition(FindEntitiesInListCommand findEntitiesInListCommand)
        {
            _findEntitiesInListCommand = findEntitiesInListCommand;
        }

        //Sitecore only adds Datetime operators out-of-the-box
        public IBinaryOperator<DateTime, DateTime> Pm_Operator { get; set; }

        //Out-of-the-box DatetimeOffset get's a nice editor and Datetime not
        public IRuleValue<DateTimeOffset> Pm_Date { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            var date = Pm_Date.Yield(context).DateTime;
            var orders = AsyncHelper.RunSync(() => context.GetOrderHistory(_findEntitiesInListCommand));

            if (orders == null)
            {
                return false;
            }

            var lastPurchaseDate = orders.Max(x => x.OrderPlacedDate).DateTime;
            return Pm_Operator.Evaluate(lastPurchaseDate, date);
        }
    }
}
