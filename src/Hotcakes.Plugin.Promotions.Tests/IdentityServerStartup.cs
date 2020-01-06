using System.Collections.Generic;
using System.Security.Claims;

using IdentityServer4.Models;
using IdentityServer4.Test;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Hotcakes.Plugin.Promotions.Tests
{
    public class IdentityServerStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            IIdentityServerBuilder builder = services.AddIdentityServer()
                                                     .AddTestUsers(new List<TestUser>
                                                     {
                                                         new TestUser
                                                         {
                                                             Username = "sitecore\\admin",
                                                             Password = "b",
                                                             IsActive = true,
                                                             SubjectId = "sitecore\\admin",
                                                             Claims = new List<Claim>
                                                             {
                                                                 new Claim("name", "admin"),
                                                                 new Claim("role", "sitecore\\Commerce Business User")
                                                             }
                                                         }
                                                     })
                                                     .AddInMemoryApiResources(new[]
                                                     {
                                                         new ApiResource("EngineAPI", new[] { "name", "email", "role" })
                                                     })
                                                     .AddInMemoryClients(new[]
                                                     {
                                                         new Client
                                                         {
                                                             ClientId = "client",

                                                             // no interactive user, use the clientid/secret for authentication
                                                             AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                                                             // secret for authentication
                                                             ClientSecrets =
                                                             {
                                                                 new Secret("secret".Sha256())
                                                             },

                                                             // scopes that client has access to
                                                             AllowedScopes = { "EngineAPI" }
                                                         }
                                                     });

            builder.AddDeveloperSigningCredential();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseIdentityServer();
        }
    }
}
