using System;
using System.Threading.Tasks;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.SQL;

namespace Hotcakes.Plugin.Promotions.Tests
{
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
}
