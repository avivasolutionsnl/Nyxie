namespace Sitecore.Commerce.Sample.Console
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using EntityViews;
    using FluentAssertions;
    using Microsoft.OData.Client;
    using Sitecore.Commerce.Sample.Contexts;
    using Sitecore.Commerce.ServiceProxy;

    public static class Versions
    {
        private static readonly Sitecore.Commerce.Engine.Container AuthoringContainer = new AnonymousCustomerJeff(EnvironmentConstants.AdventureWorksAuthoring)
            .Context.AuthoringContainer();

        public static void RunScenarios()
        {
            var watch = new Stopwatch();
            watch.Start();

            Console.WriteLine("Begin Versions");

            AddCatalogVersion();

            watch.Stop();

            Console.WriteLine($"End Versions:{watch.ElapsedMilliseconds} ms");
        }

        private static void AddCatalogVersion()
        {
            Console.WriteLine("Begin AddCatalogVersion");

            var catalogName = Guid.NewGuid().ToString("N");
            var entityId = $"Entity-Catalog-{catalogName}";

            // Create catalog
            var addCatalogView = Proxy.GetValue(AuthoringContainer.GetEntityView(string.Empty, "Details", "AddCatalog", string.Empty));
            addCatalogView.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty {Name = "Name", Value = catalogName},
                new ViewProperty {Name = "DisplayName", Value = catalogName}
            };

            var addCatalogResult = Proxy.DoCommand(AuthoringContainer.DoAction(addCatalogView));

            addCatalogResult.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

            // Create new version
            var addVersionView = Proxy.GetValue(AuthoringContainer.GetEntityView(entityId, string.Empty, "AddEntityVersion", string.Empty));
            var addVersionResult = Proxy.DoCommand(AuthoringContainer.DoAction(addVersionView));

            addVersionResult.ResponseCode.Should().Be("Ok");

            // Change merge option to retrieve all entities.
            AuthoringContainer.MergeOption = MergeOption.NoTracking;

            var versions = AuthoringContainer.FindEntityVersions("Sitecore.Commerce.Plugin.Catalog.Catalog, Sitecore.Commerce.Plugin.Catalog", entityId).Execute().ToList();

            for (int i = 1; i < versions.Count + 1; i++)
            {
                var version = versions[versions.Count - i];
                version.EntityVersion.Should().Be(i);
            }
        }
    }
}