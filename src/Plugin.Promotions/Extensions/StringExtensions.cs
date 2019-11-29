using Promethium.Plugin.Promotions.Properties;
using System;

namespace Promethium.Plugin.Promotions.Extensions
{
    internal static class StringExtensions
    {
        internal static string PrettifyOperatorName(this string displayName)
        {
            if (displayName.StartsWith("Sitecore."))
            {
                //Remove the namespace from the display name
                displayName = displayName.Substring(displayName.LastIndexOf('.') + 1);

                //Strip of the data types from the display name
                displayName = displayName.Replace("DateTime", "");
                displayName = displayName.Replace("Decimal", "");
                displayName = displayName.Replace("Double", "");
                displayName = displayName.Replace("Float", "");
                displayName = displayName.Replace("Guid", "");
                displayName = displayName.Replace("Integer", "");
                displayName = displayName.Replace("Long", "");
                displayName = displayName.Replace("String", "");

                //Lookup the user friendly value for the display name
                displayName = Resources.ResourceManager.GetString(displayName);
            }
            return displayName;
        }

        internal static bool EqualsOrdinalIgnoreCase(this string value1, string value2)
        {
            return value1.Equals(value2, StringComparison.OrdinalIgnoreCase);
        }
    }
}
