namespace Sitecore.Commerce.Extensions
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public static class ConsoleExtensions
    {
        public static void WriteColoredLine(ConsoleColor color, string text)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = originalColor;
        }

        public static void WriteErrorLine(string text)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = originalColor;
        }

        public static void WriteWarningLine(string text)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
            Console.ForegroundColor = originalColor;
        }

        public static void WriteExpectedError()
        {
            WriteColoredLine(ConsoleColor.Yellow, "The previously reported error is expected. Please ignore.");
        }
    }
}
