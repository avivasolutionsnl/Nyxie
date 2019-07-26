namespace Sitecore.Commerce.Sample.Console
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using FluentAssertions;
    using Sitecore.Commerce.Engine;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Sample.Contexts;
    using Sitecore.Commerce.ServiceProxy;
    using Sitecore.Commerce.Extensions;
    using Sitecore.Commerce.Plugin.Inventory;
    using Sitecore.Commerce.Plugin.Catalog;
    using System.IO;
    using System.Threading.Tasks;

    public static class InventoryUX
    {
        private const string Catalog1Name = "InventoryUXCatalog1";
        private const string Catalog2Name = "InventoryUXCatalog2";
        private const string InventorySet1Name = "InventoryUXInventorySet1";
        private const string InventorySet2Name = "InventoryUXInventorySet2";
        private const string ProductName = "InventoryUXProduct";
        private const int InitialQuantity = 100;
        private const int UpdatedQuantity = 200;
        private const int BackorderLimit = 50;
        private static readonly string Catalog1Id = Catalog1Name.ToEntityId<Catalog>();
        private static readonly string Catalog2Id = Catalog2Name.ToEntityId<Catalog>();
        private static readonly string InventorySet1Id = InventorySet1Name.ToEntityId<InventorySet>();
        private static readonly string InventorySet2Id = InventorySet2Name.ToEntityId<InventorySet>();
        private static readonly string ProductId = ProductName.ToEntityId<SellableItem>();
        private static readonly string ProductInventoryInfo1Id = ProductName.ToEntityId<InventoryInformation>(InventorySet1Name);
        private static readonly string ProductInventoryInfo2Id = ProductName.ToEntityId<InventoryInformation>(InventorySet2Name);
        private static readonly string InventoryExportFilePath = Path.Combine(Path.GetTempPath(), "consoleinventory.zip");
        private static readonly DateTimeOffset BackorderAvailabilityDate = DateTimeOffset.UtcNow.AddDays(1);

        public static void RunScenarios()
        {
            using (new SampleScenarioScope(nameof(InventoryUX)))
            {
                EngineExtensions.AddCatalog(Catalog1Name, $"{Catalog1Name} Display Name");
                EngineExtensions.AddCatalog(Catalog2Name, $"{Catalog2Name} Display Name");
                EngineExtensions.AddSellableItem(ProductId, Catalog1Id, Catalog1Name, Catalog1Name);
                EngineExtensions.AssociateSellableItem(ProductId, Catalog2Id, Catalog2Name, Catalog2Name);
                AddInventorySet();
                EditInventorySet();
                AssociateCatalogToInventorySet();
                AssociateSellableItemToInventorySet();
                EditInventoryInformation1();
                EditInventoryInformation2();
                ExportInventorySetsFull().Wait();
                TransferInventoryInformation();
                DisassociateSellableItemFromInventorySet();
                DisassociateCatalogFromInventorySet();
                ImportInventorySetsReplace().Wait();
                EngineExtensions.DeleteSellableItem(ProductId, Catalog1Id, Catalog1Name, Catalog1Name);
                EngineExtensions.DeleteCatalog(Catalog1Name);
            }
        }

        private static void DisassociateCatalogFromInventorySet()
        {
            using (new SampleMethodScope())
            {
                EngineExtensions.DisassociateCatalogFromInventorySet(InventorySet1Name, Catalog1Name);
                EngineExtensions.AssertChildViewItemNotExists(InventorySet1Id, ChildViewNames.InventorySetSellableItems, ProductId);

                EngineExtensions.DisassociateCatalogFromInventorySet(InventorySet2Name, Catalog2Name);
                EngineExtensions.AssertChildViewItemNotExists(InventorySet2Id, ChildViewNames.InventorySetSellableItems, ProductId);
            }
        }

        private static async Task ImportInventorySetsReplace()
        {
            using (new SampleMethodScope())
            {
                await EngineExtensions.ImportInventorySets(InventoryExportFilePath, "replace", "CatalogAlreadyAssociated");

                var inventoryInfo1 = Proxy.GetValue(EngineExtensions.AuthoringContainer.Value.InventoryInformation.ByKey(ProductInventoryInfo1Id).Expand("Components($expand=ChildComponents)"));
                inventoryInfo1.Should().NotBeNull();
                inventoryInfo1.Quantity.Should().Be(UpdatedQuantity);
                var backorderComponent1 = inventoryInfo1.Components.OfType<BackorderableComponent>().FirstOrDefault();
                backorderComponent1.Should().NotBeNull();
                backorderComponent1.Backorderable.Should().BeFalse();
                backorderComponent1.BackorderAvailabilityDate.HasValue.Should().BeFalse();
                backorderComponent1.BackorderLimit.Should().Be(0);

                var inventoryInfo2 = Proxy.GetValue(EngineExtensions.AuthoringContainer.Value.InventoryInformation.ByKey(ProductInventoryInfo2Id).Expand("Components($expand=ChildComponents)"));
                inventoryInfo2.Should().NotBeNull();
                inventoryInfo2.Quantity.Should().Be(UpdatedQuantity);
                var backorderComponent2 = inventoryInfo2.Components.OfType<BackorderableComponent>().FirstOrDefault();
                backorderComponent2.Should().NotBeNull();
                backorderComponent2.Backorderable.Should().BeTrue();
                backorderComponent2.BackorderAvailabilityDate.Should().BeCloseTo(BackorderAvailabilityDate, 1000);
                backorderComponent2.BackorderLimit.Should().Be(BackorderLimit);
            }
        }

        private static void DisassociateSellableItemFromInventorySet()
        {
            using (new SampleMethodScope())
            {
                EngineExtensions.DisassociateSellableItemFromInventorySet(InventorySet1Name, ProductId);
                EngineExtensions.DisassociateSellableItemFromInventorySet(InventorySet2Name, ProductId);
            }
        }

        private static void TransferInventoryInformation()
        {
            using (new SampleMethodScope())
            {
                var quantityToTransfer = 12;
                EngineExtensions.TransferInventoryInformation(InventorySet1Id, ProductId, InventorySet2Id, ProductId, quantityToTransfer);

                var inventoryInfo1 = Proxy.GetValue(EngineExtensions.AuthoringContainer.Value.InventoryInformation.ByKey(ProductInventoryInfo1Id));
                inventoryInfo1.Should().NotBeNull();
                inventoryInfo1.Quantity.Should().Be(UpdatedQuantity - quantityToTransfer);

                var inventoryInfo2 = Proxy.GetValue(EngineExtensions.AuthoringContainer.Value.InventoryInformation.ByKey(ProductInventoryInfo2Id));
                inventoryInfo2.Should().NotBeNull();
                inventoryInfo2.Quantity.Should().Be(UpdatedQuantity + quantityToTransfer);
            }
        }

        private static async Task ExportInventorySetsFull()
        {
            using (new SampleMethodScope())
            {
                await EngineExtensions.ExportInventorySets(InventoryExportFilePath, "full");
            }
        }

        private static void EditInventoryInformation1()
        {
            using (new SampleMethodScope())
            {
                EngineExtensions.EditInventoryInformation(InventorySet1Name, ProductId, new System.Collections.Generic.List<ViewProperty>
                {
                    new ViewProperty { Name = "Quantity", Value = UpdatedQuantity.ToString(), OriginalType = typeof(int).FullName }
                });

                var inventoryInfo1 = Proxy.GetValue(EngineExtensions.AuthoringContainer.Value.InventoryInformation.ByKey(ProductInventoryInfo1Id).Expand("Components($expand=ChildComponents)"));
                inventoryInfo1.Should().NotBeNull();
                inventoryInfo1.Quantity.Should().Be(UpdatedQuantity);
                var backorderComponent1 = inventoryInfo1.Components.OfType<BackorderableComponent>().FirstOrDefault();
                backorderComponent1.Should().NotBeNull();
                backorderComponent1.Backorderable.Should().BeFalse();
                backorderComponent1.BackorderAvailabilityDate.HasValue.Should().BeFalse();
                backorderComponent1.BackorderLimit.Should().Be(0);
            }
        }

        private static void EditInventoryInformation2()
        {
            using (new SampleMethodScope())
            {
                EngineExtensions.EditInventoryInformation(InventorySet2Name, ProductId, new System.Collections.Generic.List<ViewProperty>
                {
                    new ViewProperty { Name = "Quantity", Value = UpdatedQuantity.ToString(), OriginalType = typeof(int).FullName },
                    new ViewProperty { Name = "Backorderable", Value = "true", OriginalType = typeof(bool).FullName },
                    new ViewProperty { Name = "BackorderAvailabilityDate", Value = BackorderAvailabilityDate.ToString(), OriginalType = typeof(DateTimeOffset).FullName },
                    new ViewProperty { Name = "BackorderLimit", Value = BackorderLimit.ToString(), OriginalType = typeof(int).FullName }
                });

                var inventoryInfo2 = Proxy.GetValue(EngineExtensions.AuthoringContainer.Value.InventoryInformation.ByKey(ProductInventoryInfo2Id).Expand("Components($expand=ChildComponents)"));
                inventoryInfo2.Should().NotBeNull();
                inventoryInfo2.Quantity.Should().Be(UpdatedQuantity);
                var backorderComponent2 = inventoryInfo2.Components.OfType<BackorderableComponent>().FirstOrDefault();
                backorderComponent2.Should().NotBeNull();
                backorderComponent2.Backorderable.Should().BeTrue();
                backorderComponent2.BackorderAvailabilityDate.HasValue.Should().BeTrue();
                backorderComponent2.BackorderAvailabilityDate.Value.Should().BeCloseTo(BackorderAvailabilityDate, 1000);
                backorderComponent2.BackorderLimit.Should().Be(BackorderLimit);
            }
        }

        private static void AssociateSellableItemToInventorySet()
        {
            using (new SampleMethodScope())
            {
                EngineExtensions.AssociateSellableItemToInventorySet(InventorySet1Name, ProductId, InitialQuantity);
                EngineExtensions.AssociateSellableItemToInventorySet(InventorySet2Name, ProductId, InitialQuantity);
            }
        }

        private static void AssociateCatalogToInventorySet()
        {
            using (new SampleMethodScope())
            {
                EngineExtensions.AssociateCatalogToInventorySet(InventorySet1Name, Catalog1Name);
                EngineExtensions.AssociateCatalogToInventorySet(InventorySet2Name, Catalog2Name);
            }
        }

        private static void AddInventorySet()
        {
            using (new SampleMethodScope())
            {
                EngineExtensions.AddInventorySet(InventorySet1Name);
                EngineExtensions.AddInventorySet(InventorySet2Name);
            }
        }

        private static void EditInventorySet()
        {
            using (new SampleMethodScope())
            {
                var view = Proxy.GetValue(EngineExtensions.AuthoringContainer.Value.GetEntityView(InventorySet1Id, "Details", "EditInventorySet", string.Empty));
                view.Should().NotBeNull();
                view.Policies.Should().BeEmpty();
                view.Properties.Should().NotBeEmpty();
                view.ChildViews.Should().BeEmpty();

                var version = view.Properties.FirstOrDefault(p => p.Name.Equals("Version"));

                view.Properties = new ObservableCollection<ViewProperty>
                {
                    new ViewProperty {Name = "DisplayName", Value = "Console UX Inventory Set (updated)"},
                    new ViewProperty {Name = "Description", Value = "Console UX Inventory Set Description"},
                    version
                };

                var result = Proxy.DoCommand(EngineExtensions.AuthoringContainer.Value.DoAction(view));
                result.Messages.Any(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();
            }
        }
    }
}
