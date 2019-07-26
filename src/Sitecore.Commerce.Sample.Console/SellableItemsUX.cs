namespace Sitecore.Commerce.Sample.Console
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using FluentAssertions;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Extensions;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Sample.Contexts;
    using Sitecore.Commerce.ServiceProxy;
    using System.Reflection;
    using System.Collections.ObjectModel;
    using Sitecore.Commerce.EntityViews;

    public static class SellableItemsUX
    {
        internal const string CatalogName = "Adventure Works Catalog";
        internal const string CategoryName = "SellableItemUXCategory";
        internal const string Product1Name = "ConsoleProduct1";
        internal const string Product2Name = "ConsoleProduct2";
        internal const string Variant1Name = "ConsoleVariant1";
        internal const string ImageId = "f6ba4fec-07e7-4c69-858e-6acd5eba4c1b";
        internal static readonly string CatalogId = CatalogName.ToEntityId<Catalog>();
        internal static readonly string CategoryId = CategoryName.ToEntityId<Category>(CatalogName);
        internal static readonly string Product1Id = Product1Name.ToEntityId<SellableItem>();
        internal static readonly string Product2Id = Product2Name.ToEntityId<SellableItem>();

        public static void RunScenarios()
        {
            using (new SampleScenarioScope(MethodBase.GetCurrentMethod().DeclaringType.Name))
            {
                EngineExtensions.AddCategory(CategoryId, CatalogId, CatalogName);
                AddSellableItemToCatalog();
                AddSellableItemToCategory();
                AddSellableItemVariant();
                DisableSellableItemVariant();
                EnableSellableItemVariant();
                DeleteSellableItemVariant();
                AssociateSellableItemToCatalog();
                AssociateSellableItemToCategory();
                DissassociateSellableItemFromCatalog();
                DissassociateSellableItemFromCategory();
                AddSellableItemImage();
                RemoveSellableItemImage();
                EngineExtensions.DeleteSellableItem(Product1Id, CategoryId, CategoryName, CatalogName);
                EngineExtensions.DeleteSellableItem(Product2Id, CategoryId, CategoryName, CatalogName);
                EngineExtensions.DeleteCategory(CategoryId);
            }
        }

        private static void RemoveSellableItemImage()
        {
            using (new SampleMethodScope())
            {
                EngineExtensions.RemoveSellableItemImage(Product1Id, CatalogName, ImageId);
            }
        }

        private static void AddSellableItemImage()
        {
            using (new SampleMethodScope())
            {
                EngineExtensions.AddSellableItemImage(Product1Id, CatalogName, ImageId);
            }
        }

        private static void DissassociateSellableItemFromCategory()
        {
            using (new SampleMethodScope())
            {
                // Remove Product1 from the category
                EngineExtensions.AssertChildViewItemExists(CategoryId, ChildViewNames.SellableItems, Product1Id);
                EngineExtensions.DisassociateItem(Product1Id, Product1Name, CategoryId, CategoryName);
            }
        }

        private static void DissassociateSellableItemFromCatalog()
        {
            using (new SampleMethodScope())
            {
                // Remove Product2 from the catalog root
                EngineExtensions.AssertChildViewItemExists(CatalogId, ChildViewNames.SellableItems, Product2Id);
                EngineExtensions.DisassociateItem(Product2Id, Product2Name, CatalogId, CatalogName);
            }
        }

        private static void AssociateSellableItemToCategory()
        {
            using (new SampleMethodScope())
            {
                // Product1 should be a child of the catalog, but not the category.
                EngineExtensions.AssertChildViewItemExists(CatalogId, ChildViewNames.SellableItems, Product1Id);
                EngineExtensions.AssertChildViewItemNotExists(CategoryId, ChildViewNames.SellableItems, Product1Id);

                // Add Product1 to the category
                EngineExtensions.AssociateSellableItem(Product1Id, CategoryId, CategoryName, CatalogName);
            }
        }

        private static void AssociateSellableItemToCatalog()
        {
            using (new SampleMethodScope())
            {
                // Product2 should be a child of the category, but not the catalog.
                EngineExtensions.AssertChildViewItemExists(CategoryId, ChildViewNames.SellableItems, Product2Id);
                EngineExtensions.AssertChildViewItemNotExists(CatalogId, ChildViewNames.SellableItems, Product2Id);

                // Add Product2 to the catalog root
                EngineExtensions.AssociateSellableItem(Product2Id, CatalogId, CatalogName, CatalogName);
            }
        }

        private static void DeleteSellableItemVariant()
        {
            using (new SampleMethodScope())
            {
                EngineExtensions.DeleteSellableItemVariant(Variant1Name, Product1Name, CatalogName, CatalogName);
            }
        }

        private static void DisableSellableItemVariant()
        {
            using (new SampleMethodScope())
            {
                EngineExtensions.DisableSellableItemVariant(Variant1Name, Product1Name, CatalogName, CatalogName);
            }
        }

        private static void EnableSellableItemVariant()
        {
            using (new SampleMethodScope())
            {
                EngineExtensions.EnableSellableItemVariant(Variant1Name, Product1Name, CatalogName, CatalogName);
            }
        }

        private static void AddSellableItemVariant()
        {
            using (new SampleMethodScope())
            {
                EngineExtensions.AddSellableItemVariant(Variant1Name, Product1Name, CatalogName, CatalogName);
            }
        }

        private static void AddSellableItemToCatalog()
        {
            using (new SampleMethodScope())
            {
                // Add Product1 to the catalog root.
                EngineExtensions.AssertCatalogExists(CatalogId);
                EngineExtensions.AddSellableItem(Product1Id, CatalogId, CatalogName, CatalogName);
            }
        }

        private static void AddSellableItemToCategory()
        {
            using (new SampleMethodScope())
            {
                // Add Product2 to the category.
                EngineExtensions.AssertCategoryExists(CategoryId);
                EngineExtensions.AddSellableItem(Product2Id, CategoryId, CategoryName, CatalogName);
            }
        }
    }
}
