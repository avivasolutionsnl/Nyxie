using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Hotcakes.Plugin.Promotions.Tests.Builders
{
    public class IsCurrentDayConditionBuilder : IQualificationBuilder
    {
        private int day;

        public IsCurrentDayConditionBuilder Day(int day)
        {
            this.day = day;
            return this;
        }

        public ConditionModel Build()
        {
            return new ConditionModel
            {
                Name = "IsCurrentDayCondition",
                LibraryId = "IsCurrentDayCondition",
                Properties = new List<PropertyModel>
                {
                    new PropertyModel
                    {
                        Name = "Day",
                        Value = day.ToString()
                    }
                }
            };
        }
    }
}
