namespace Sitecore.Commerce.Sample.Console
{
    using Sitecore.Commerce.Extensions;
    using System;
    using System.Diagnostics;
    using Sitecore.Commerce.Sample.Console.Authentication;

    public class Program
    {
        // Should the environment be bootstrapped when this program runs?
        private static readonly bool ShouldBootstrapOnLoad = true;
        private static readonly bool ShouldDevOpsScenarios = true;
        private static readonly bool ShouldRunPricingScenarios = true;
        private static readonly bool ShouldRunPromotionsScenarios = true;
        private static readonly bool ShouldRunCatalogScenarios = true;
        private static readonly bool ShouldRunInventoryScenarios = true;
        private static readonly bool ShouldRunOrdersScenarios = true;
        private static readonly bool ShouldRunCustomersScenarios = true;
        private static readonly bool ShouldRunEntitlementsScenarios = true;
        private static readonly bool ShouldRunSearchScenarios = false;
        private static readonly bool ShouldRunBusinessUsersScenarios = true;
        private static readonly bool ShouldRunVersionScenarios = true;
        
        private static readonly bool DemoStops = true;
        
        public static string CurrentEnvironment = "AdventureWorksShops";
        public static string DefaultStorefront = "CommerceEngineDefaultStorefront";

        public static string OpsServiceUri = "https://localhost:5000/CommerceOps/";
        public static string ShopsServiceUri = "https://localhost:5000/api/";
        public static string MinionsServiceUri = "https://localhost:5000/CommerceOps/";
        public static string AuthoringServiceUri = "https://localhost:5000/api/";
        public static string SitecoreIdServerUri = "https://sxastorefront-identityserver/";

        public static string UserName = @"sitecore\admin";
        public static string Password = "b";

        public static string SitecoreTokenRaw;
        public static string SitecoreToken;

        static void Main(string[] args)
        {
            try
            {
                OpsServiceUri = Properties.Settings.Default.OpsServiceUri;
                ShopsServiceUri = Properties.Settings.Default.ShopsServiceUri;
                MinionsServiceUri = Properties.Settings.Default.MinionsServiceUri;
                AuthoringServiceUri = Properties.Settings.Default.AuthoringServiceUri;
                SitecoreIdServerUri = Properties.Settings.Default.SitecoreIdServerUri;

                UserName = Properties.Settings.Default.UserName;
                Password = Properties.Settings.Default.Password;

                SitecoreTokenRaw = SitecoreIdServerAuth.GetToken();
                SitecoreToken = $"Bearer {SitecoreTokenRaw}";

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                Console.ForegroundColor = ConsoleColor.Cyan;

                if (ShouldBootstrapOnLoad)
                {
                    Bootstrapping.RunScenarios();
                    Content.RunScenarios();
                }

                if (ShouldDevOpsScenarios)
                {
                    Environments.RunScenarios();

                    Plugins.RunScenarios();

                    Entities.RunScenarios();

                    Policies.RunScenarios();

                    Caching.RunScenarios();
                }

                if (ShouldRunCatalogScenarios)
                {
                    Catalogs.RunScenarios();
                    CatalogsUX.RunScenarios();

                    Categories.RunScenarios();
                    CategoriesUX.RunScenarios();

                    // TODO: contains failing tests
                    SellableItems.RunScenarios();

                    SellableItemsUX.RunScenarios();
                }

                if (ShouldRunPricingScenarios)
                {
                    Pricing.RunScenarios();
                    PricingUX.RunScenarios();
                }

                if (ShouldRunPromotionsScenarios)
                {
                    Promotions.RunScenarios();
                    PromotionsUX.RunScenarios();
                    PromotionsRuntime.RunScenarios();

                    Rules.RunScenarios();

                    Coupons.RunScenarios();
                    CouponsUX.RunScenarios();
                }

                if (ShouldRunInventoryScenarios)
                {
                    Inventory.RunScenarios();
                    InventoryUX.RunScenarios();
                }

                if (ShouldRunOrdersScenarios)
                {
                    Fulfillments.RunScenarios();

                    Payments.RunScenarios();
                    PaymentsFederated.RunScenarios();

                    Carts.RunScenarios();

                    Returns.RunScenarios();

                    OrdersUX.RunScenarios();
                    Orders.RunScenarios();

                    Shipments.RunScenarios(); // ORDERS HAVE TO BE RELEASED FOR SHIPMENTS TO GET GENERATED
                }

                if (ShouldRunCustomersScenarios)
                {
                    CustomersUX.RunScenarios();
                }

                if (ShouldRunEntitlementsScenarios)
                {
                    Entitlements.RunScenarios();
                }

                if (ShouldRunSearchScenarios)
                {
                    Search.RunScenarios();
                }

                if (ShouldRunBusinessUsersScenarios)
                {
                    ComposerUX.RunScenarios();
                    // Composer.RunScenarios();
                }

                if (ShouldRunVersionScenarios)
                {
                    Versions.RunScenarios();
                }

                stopwatch.Stop();

                Console.WriteLine($"Test Runs Complete - {stopwatch.ElapsedMilliseconds} ms -  (Hit any key to continue)");

                if (DemoStops)
                {
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                ConsoleExtensions.WriteErrorLine("An unexpected exception occurred.");
                ConsoleExtensions.WriteErrorLine(ex.ToString());
            }

            Console.WriteLine("done.");
        }
    }
}