using System.Collections.Generic;
using Sitecore.Commerce.Core;

namespace Promethium.Plugin.Promotions
{
    internal sealed class BasicStringComparer
    {
        private const string EqualName = "Sitecore.Framework.Rules.StringEqualityOperator";
        private const string NotEqualName = "Sitecore.Framework.Rules.StringNotEqualityOperator";

        static BasicStringComparer()
        {
            //Don't get the display names from resources. This is the same text as Sitecore uses and is processed in "PrettifySelectOptionsBlock"
            Options = new List<Selection>
            {
                new Selection { DisplayName = "Sitecore.Core.Operators.StringEqualityOperator", Name = EqualName },
                new Selection { DisplayName = "Sitecore.Core.Operators.StringNotEqualityOperator", Name = NotEqualName },
            };
        }

        internal static List<Selection> Options { get; set; }

        internal static bool Evaluate(string comparer, string value1, string value2)
        {
            switch (comparer)
            {
                case EqualName:
                    return value1 == value2;
                case NotEqualName:
                    return value1 != value2;
                default:
                    return false;
            }
        }
    }
}
