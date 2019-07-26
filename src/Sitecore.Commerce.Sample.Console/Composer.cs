namespace Sitecore.Commerce.Sample.Console
{
    using System;
    using System.Diagnostics;
    using FluentAssertions;

    using Sitecore.Commerce.Plugin.Composer;

    using Sitecore.Commerce.Sample.Contexts;
    using Sitecore.Commerce.ServiceProxy;

    public static class Composer
    {
        private static Sitecore.Commerce.Engine.Container _authoringContainer;

        public  static void RunScenarios()
        {
            var watch = new Stopwatch();
            watch.Start();

            Console.WriteLine("Begin Composer");

            var context = new CsrSheila().Context;
            context.Environment = "AdventureWorksAuthoring";
            _authoringContainer = context.ShopsContainer();

            GetComposerTemplate("MyConsoleTemplate");
            GetComposerTemplates();
            
            watch.Stop();

            Console.WriteLine($"End PricingBooksAndCards :{watch.ElapsedMilliseconds} ms");
        }
        
        private static void GetComposerTemplate(string templateName)
        {
            Console.WriteLine("Begin GetComposerTemplate");
          
            var result = Proxy.GetValue(_authoringContainer.ComposerTemplates.ByKey(templateName).Expand("Components"));
            result.Should().NotBeNull();

            result = Proxy.GetValue(_authoringContainer.ComposerTemplates.ByKey($"Entity-ComposerTemplate-{templateName}").Expand("Components"));
            result.Should().NotBeNull();
        }
        
        private static void GetComposerTemplates()
        {
            Console.WriteLine("Begin GetComposerTemplate");
            
            var result = _authoringContainer.ComposerTemplates.Expand("Components").Execute();
            result.Should().NotBeNull();
        }
    }
}
