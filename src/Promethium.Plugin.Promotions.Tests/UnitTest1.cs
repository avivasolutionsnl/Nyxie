using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using IdentityModel.Client;

using IdentityServer4.AccessTokenValidation;
using IdentityServer4.Models;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Engine;
using Sitecore.Commerce.Plugin.SQL;

using Xunit;
using Xunit.Abstractions;

namespace Promethium.Plugin.Promotions.Tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper testOutputHelper;

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async void Test1()
        {
            var builder = new WebHostBuilder()//.ConfigureLogging(c => { c.AddProvider(new XunitLoggerProvider(testOutputHelper)); })
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

            SetEntryAssembly<Startup>();

            var factory = new WebAppFactory()
                .WithWebHostBuilder(b =>
                {
                    b.ConfigureLogging(c => { c.AddProvider(new XunitLoggerProvider(testOutputHelper)); });
                    b.ConfigureServices(c => { c.Configure<IdentityServerAuthenticationOptions>("Bearer", options =>
                    {
                        options.Authority = "http://localhost";

                        // IMPORTANT PART HERE
                        options.JwtBackChannelHandler = server.CreateHandler();
                        options.IntrospectionDiscoveryHandler = server.CreateHandler();
                        options.IntrospectionBackChannelHandler = server.CreateHandler();
                    }); });

                    b.ConfigureAppConfiguration((c, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string>
                        {
                            { "AppSettings:SitecoreIdentityServerUrl", "http://localhost" }
                        });
                    });
                });

            var client = factory.CreateClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            
            var message = new HttpRequestMessage(HttpMethod.Get, "api/Carts('Cart01')");
            

            var response = await client.SendAsync(message);

            Assert.True(response.IsSuccessStatusCode);
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

    public class WebAppFactory : WebApplicationFactory<Startup>
    {
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseWebRoot(Path.GetFullPath("wwwroot"));

            builder.ConfigureAppConfiguration((context, b) =>
            { 
                b.SetBasePath(Path.GetFullPath("wwwroot"))
                       .AddJsonFile("config.json", false, true);
            });

            base.ConfigureWebHost(builder);

            builder.UseSolutionRelativeContentRoot("")
                .UseStartup<Startup>();

            builder.ConfigureTestServices(c =>
            {
                c.AddTransient<GetDatabaseVersionCommand, DummyGetDatabaseVersionCommand>();
                new Persistence.ConfigureSitecore().ConfigureServices(c);
            });
        }
    }

    public class DummyGetDatabaseVersionCommand : GetDatabaseVersionCommand
    {
        public DummyGetDatabaseVersionCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override Task<string> Process(CommerceContext commerceContext)
        {
            return Task.FromResult("9.1.0");
        }
    }

    public class TestStartup : Startup
    {
        public TestStartup(IServiceProvider serviceProvider, IHostingEnvironment hostEnv, IConfiguration configuration) : base(serviceProvider, hostEnv, configuration)
        {
        }
    }

    public class XunitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public XunitLoggerProvider(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public ILogger CreateLogger(string categoryName)
            => new XunitLogger(_testOutputHelper, categoryName);

        public void Dispose()
        { }
    }

    public class XunitLogger : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly string _categoryName;

        public XunitLogger(ITestOutputHelper testOutputHelper, string categoryName)
        {
            _testOutputHelper = testOutputHelper;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
            => NoopDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _testOutputHelper.WriteLine($"{_categoryName} [{eventId}] {formatter(state, exception)}");
            if (exception != null)
                _testOutputHelper.WriteLine(exception.ToString());
        }

        private class NoopDisposable : IDisposable
        {
            public static NoopDisposable Instance = new NoopDisposable();
            public void Dispose()
            { }
        }
    }
}
