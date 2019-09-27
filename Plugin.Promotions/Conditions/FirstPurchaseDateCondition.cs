using Sitecore.Framework.Rules;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Customers;

namespace Promethium.Plugin.Promotions.Conditions
{
    /// <summary>
    /// A SiteCore Commerce condition for the qualification
    /// "Current Customer first purchase is [operator] [date]"
    /// </summary>
    [EntityIdentifier("Promethium_" + nameof(FirstPurchaseDateCondition))]
    public class FirstPurchaseDateCondition : ICondition, ICustomerCondition
    {
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;

        public FirstPurchaseDateCondition(FindEntitiesInListCommand findEntitiesInListCommand)
        {
            _findEntitiesInListCommand = findEntitiesInListCommand;
        }

        public IBinaryOperator<DateTimeOffset, DateTimeOffset> Operator { get; set; }

        public IRuleValue<DateTimeOffset> Date { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            var date = Date.Yield(context);

            if (!context.GetOrderHistory(_findEntitiesInListCommand, out var orders))
            {
                return false;
            }

            var firstPurchaseDate = orders.Min(x => x.OrderPlacedDate);
            return Operator.Evaluate(firstPurchaseDate, date);
        }
    }
}
