namespace Sitecore.Commerce.Sample.Console
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Extensions;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Sample.Contexts;
    using Sitecore.Commerce.ServiceProxy;

    public static class CatalogsUX
    {
        public const string CatalogName = "ConsoleCatalog";
        public static readonly string CatalogId = $"Entity-Catalog-{CatalogName}";
        public const string CatalogCloneName = "ConsoleCatalogClone";
        public static readonly string CatalogCloneId = $"Entity-Catalog-{CatalogCloneName}";
        public static readonly string CatalogExportFilePath = Path.Combine(Path.GetTempPath(), "consolecatalog.zip");

        public static void RunScenarios()
        {
            var shopsContainer = new AnonymousCustomerJeff().Context.ShopsContainer();
            var authoringContainer = new AnonymousCustomerJeff(EnvironmentConstants.AdventureWorksAuthoring)
                .Context.AuthoringContainer();

            using (new SampleScenarioScope(MethodBase.GetCurrentMethod().DeclaringType.Name))
            {
                AddCatalog();
                EditCatalogUsingAuthoringEnvironment(authoringContainer);
                TryEditCatalogUsingShopsEnvironment(shopsContainer);
                ExportCatalogsFull().Wait();
                DeleteCatalog();
                ImportCatalogsReplace().Wait();
                CloneCatalog();
            }
        }

        private static void AddCatalog()
        {
            using (new SampleMethodScope())
            {
                // test for validation error
                var view = Proxy.GetValue(EngineExtensions.AuthoringContainer.Value.GetEntityView(string.Empty, "Details", "AddCatalog", string.Empty));
                view.Should().NotBeNull();
                view.Policies.Should().BeEmpty();
                view.Properties.Should().NotBeEmpty();
                view.ChildViews.Should().BeEmpty();

                view.Properties = new ObservableCollection<ViewProperty>
                {
                    new ViewProperty {Name = "Name", Value = $"{CatalogName}$%^*&{{"},
                };

                var result = Proxy.DoCommand(EngineExtensions.AuthoringContainer.Value.DoAction(view));
                result.Messages.Should().ContainMessageCode("validationerror");
                ConsoleExtensions.WriteExpectedError();

                // Create a valid catalog
                EngineExtensions.AddCatalog(CatalogName, "Console UX Catalog");
            }
        }

        private static void TryEditCatalogUsingShopsEnvironment(Sitecore.Commerce.Engine.Container container)
        {
            var view = Proxy.GetValue(container.GetEntityView(CatalogId, "Details", "EditCatalog", string.Empty));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().BeEmpty();
            view.ChildViews.Should().BeEmpty();
        }

        private static void EditCatalogUsingAuthoringEnvironment(Sitecore.Commerce.Engine.Container container)
        {
            using (new SampleMethodScope())
            {
                Console.WriteLine("Begin EditCatalog");

                var view = Proxy.GetValue(container.GetEntityView(CatalogId, "Details", "EditCatalog", string.Empty));
                view.Should().NotBeNull();
                view.Policies.Should().BeEmpty();
                view.Properties.Should().NotBeEmpty();
                view.ChildViews.Should().BeEmpty();

                var version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));

                view.Properties = new ObservableCollection<ViewProperty>
                {
                    new ViewProperty {Name = "DisplayName", Value = "Console UX Catalog (updated)"},
                    version
                };

                var result = Proxy.DoCommand(container.DoAction(view));
                result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            }
        }

        private static async Task ExportCatalogsFull()
        {
            using (new SampleMethodScope())
            {
                var getCatalogResult = Proxy.GetValue(EngineExtensions.AuthoringContainer.Value.Catalogs.ByKey(CatalogName));
                getCatalogResult.Should().NotBeNull();

                await EngineExtensions.ExportCatalogs(CatalogExportFilePath);
            }
        }

        private static void DeleteCatalog()
        {
            using (new SampleMethodScope())
            {
                EngineExtensions.DeleteCatalog(CatalogName);
            }
        }

        private static async Task ImportCatalogsReplace()
        {
            using (new SampleMethodScope())
            {
                // The catalog ConsoleCatalog should not exist before performing the import.
                var getCatalogResult = Proxy.GetValue(EngineExtensions.AuthoringContainer.Value.Catalogs.ByKey(CatalogName));
                getCatalogResult.Should().BeNull();
                ConsoleExtensions.WriteExpectedError();

                await EngineExtensions.ImportCatalogs(CatalogExportFilePath, "replace");

                getCatalogResult = Proxy.GetValue(EngineExtensions.AuthoringContainer.Value.Catalogs.ByKey(CatalogName));
                getCatalogResult.Should().NotBeNull();

                // Temporary workaround for 9.0.3 bug - need to manually associate
                // inventory, price book, and promotion book to catalog after import
                EngineExtensions.AssociateCatalogToInventorySet("Adventure Works Inventory", "Adventure Works Catalog");
                Proxy.DoCommand(EngineExtensions.AuthoringContainer.Value.AssociateCatalogToPriceBook("AdventureWorksPriceBook", "Adventure Works Catalog"));
                var view = Proxy.GetValue(EngineExtensions.AuthoringContainer.Value.GetEntityView("Entity-PromotionBook-AdventureWorksPromotionBook", "PromotionBookCatalogs", "AssociateCatalog", string.Empty));
                var version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));
                view.Properties = new ObservableCollection<ViewProperty>
                {
                    new ViewProperty { Value = "Adventure Works Catalog", Name = "CatalogName" },
                    version
                };
                var result = Proxy.DoCommand(EngineExtensions.AuthoringContainer.Value.DoAction(view));
            }
        }

        private static void CloneCatalog()
        {
            using (new SampleMethodScope())
            {
                EngineExtensions.DeleteCatalogIfExists(CatalogCloneName);

                var cloneResult = Proxy.GetValue(EngineExtensions.AuthoringContainer.Value.CloneCatalog(CatalogName, CatalogCloneName));
                cloneResult.WaitUntilCompletion();

                var getCatalogResult = Proxy.GetValue(EngineExtensions.AuthoringContainer.Value.Catalogs.ByKey(CatalogCloneName));
                getCatalogResult.Should().NotBeNull();
            }
        }
    }
}
