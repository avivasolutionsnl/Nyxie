using System;
using System.Reflection;
using System.Threading.Tasks;

using IdentityModel.Client;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;

using Sitecore.Commerce.Engine;

using Xunit;
using Xunit.Abstractions;

namespace Promethium.Plugin.Promotions.Tests
{
    public class EngineFixture : IAsyncLifetime
    {
        private ITestOutputHelper testOutputHelper;
        
        public string AccessToken { get; private set; }
        
        public AuthenticatedWebAppFactory Factory { get; private set; }

        public void SetOutput(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }
        
        public async Task InitializeAsync()
        {
            SetEntryAssembly<Startup>();

            var builder = new WebHostBuilder()
                .UseStartup<IdentityServerStartup>();
            var server = new TestServer(builder);

            var idClient = server.CreateClient();

            var disco = await idClient.GetDiscoveryDocumentAsync();
            
            var tokenResponse = await idClient.RequestPasswordTokenAsync(new PasswordTokenRequest
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

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
