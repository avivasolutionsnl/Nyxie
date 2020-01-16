using System.Collections.Generic;

using Sitecore.Commerce.Plugin.Rules;

namespace Nyxie.Plugin.Promotions.Tests.Builders
{
    public class IsCurrentDayConditionBuilder : IQualificationBuilder
    {
        private int day;

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

        public IsCurrentDayConditionBuilder Day(int day)
        {
            this.day = day;
            return this;
        }
    }
}
