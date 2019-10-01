﻿using Sitecore.Framework.Rules;
using System;
using System.Linq;
using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Customers;

namespace Promethium.Plugin.Promotions.Conditions
{
    /// <summary>
    /// A SiteCore Commerce condition for the qualification
    /// "Current Customer last purchase is [operator] [date]"
    /// </summary>
    [EntityIdentifier("Promethium_" + nameof(LastPurchaseDateCondition))]
    public class LastPurchaseDateCondition : ICondition, ICustomerCondition
    {
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;

        public LastPurchaseDateCondition(FindEntitiesInListCommand findEntitiesInListCommand)
        {
            _findEntitiesInListCommand = findEntitiesInListCommand;
        }

        //SiteCore only adds Datetime operators out-of-the-box
        public IBinaryOperator<DateTime, DateTime> Operator { get; set; }

        //Out-of-the-box DatetimeOffset get's a nice editor and Datetime not
        public IRuleValue<DateTimeOffset> Date { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            var date = Date.Yield(context).DateTime;

            if (!context.GetOrderHistory(_findEntitiesInListCommand, out var orders))
            {
                return false;
            }

            var lastPurchaseDate = orders.Max(x => x.OrderPlacedDate).DateTime;
            return Operator.Evaluate(lastPurchaseDate, date);
        }
    }
}