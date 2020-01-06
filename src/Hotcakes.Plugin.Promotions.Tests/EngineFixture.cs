using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using IdentityModel.Client;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

using Sitecore.Commerce.Engine;

using Xunit;
using Xunit.Abstractions;

namespace Hotcakes.Plugin.Promotions.Tests
{
    public class EngineFixture : IAsyncLifetime
    {
        private ITestOutputHelper testOutputHelper;

        public string AccessToken { get; private set; }

        public AuthenticatedWebAppFactory Factory { get; private set; }

        public async Task InitializeAsync()
        {
            SetEntryAssembly<Startup>();

            IWebHostBuilder builder = new WebHostBuilder()
                .UseStartup<IdentityServerStartup>();
            var server = new TestServer(builder);

            HttpClient idClient = server.CreateClient();

            DiscoveryResponse disco = await idClient.GetDiscoveryDocumentAsync();

            TokenResponse tokenResponse = await idClient.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                UserName = "sitecore\\admin",
                Password = "b",
                ClientId = "client",
                Scope = "EngineAPI",
                ClientSecret = "secret"
            });

            AccessToken = tokenResponse.AccessToken;

            Factory = new AuthenticatedWebAppFactory(AccessToken, server.CreateHandler(), () => testOutputHelper);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public void SetOutput(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        private static void SetEntryAssembly<T>()
        {
            Assembly assembly = typeof(T).Assembly;

            var manager = new AppDomainManager();
            FieldInfo entryAssemblyfield =
                manager.GetType().GetField("m_entryAssembly", BindingFlags.Instance | BindingFlags.NonPublic);
            entryAssemblyfield.SetValue(manager, assembly);

            AppDomain domain = AppDomain.CurrentDomain;
            FieldInfo domainManagerField =
                domain.GetType().GetField("_domainManager", BindingFlags.Instance | BindingFlags.NonPublic);
            domainManagerField.SetValue(domain, manager);
        }
    }
}
