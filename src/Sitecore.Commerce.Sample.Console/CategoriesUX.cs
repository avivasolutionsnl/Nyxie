namespace Sitecore.Commerce.Sample.Console
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using FluentAssertions;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Extensions;
    using Sitecore.Commerce.Sample.Contexts;
    using Sitecore.Commerce.ServiceProxy;

    public static class CategoriesUX
    {
        internal const string CatalogName = "Adventure Works Catalog";
        internal const string ParentCategoryName = "ConsoleCategory";
        internal const string ChildCategoryName = "ConsoleChildCategory";
        internal static readonly string CatalogId = $"Entity-Catalog-{CatalogName}";
        internal static readonly string ParentCategoryId = $"Entity-Category-{CatalogName}-{ParentCategoryName}";
        internal static readonly string ChildCategoryId = $"Entity-Category-{CatalogName}-{ChildCategoryName}";

        public static void RunScenarios()
        {
            using (new SampleScenarioScope(MethodBase.GetCurrentMethod().DeclaringType.Name))
            {
                AddCategoryToCatalog();
                AddCategoryToCategory();
                EditCategory();
                TryEditCategoryUsingShopsEnvironment();
                EngineExtensions.DeleteCategory(ChildCategoryId);
                EngineExtensions.DeleteCategory(ParentCategoryId);
            }
        }

        private static void AddCategoryToCatalog()
        {
            using (new SampleMethodScope())
            {
                EngineExtensions.AssertCatalogExists(CatalogId);
                EngineExtensions.AddCategory(ParentCategoryId, CatalogId, CatalogName);
            }
        }

        private static void AddCategoryToCategory()
        {
            using (new SampleMethodScope())
            {
                EngineExtensions.AssertCategoryExists(ParentCategoryId);
                EngineExtensions.AddCategory(ChildCategoryId, ParentCategoryId, ParentCategoryName);
            }
        }

        private static void TryEditCategoryUsingShopsEnvironment()
        {
            var shopsContainer = new AnonymousCustomerJeff().Context.ShopsContainer();
            var view = Proxy.GetValue(shopsContainer.GetEntityView(ParentCategoryId, "Details", "EditCategory", string.Empty));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().BeEmpty();
            view.ChildViews.Should().BeEmpty();
        }

        private static void EditCategory()
        {
            using (new SampleMethodScope())
            {
                var view = Proxy.GetValue(EngineExtensions.AuthoringContainer.Value.GetEntityView(ParentCategoryId, "Details", "EditCategory", string.Empty));
                view.Should().NotBeNull();
                view.Policies.Should().BeEmpty();
                view.Properties.Should().NotBeEmpty();
                view.ChildViews.Should().BeEmpty();

                var version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));

                view.Properties = new ObservableCollection<ViewProperty>
                {
                    new ViewProperty {Name = "DisplayName", Value = "Console UX Category (updated)"},
                    new ViewProperty {Name = "Description", Value = "Console UX Category Description"},
                    version
                };

                var result = Proxy.DoCommand(EngineExtensions.AuthoringContainer.Value.DoAction(view));
                result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            }
        }
    }
}
