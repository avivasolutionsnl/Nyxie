using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sitecore.Commerce.Core;

namespace Promethium.Plugin.Promotions.Tests.Persistence
{
    public interface IStore
    {
        void Add(CommerceEntity entity);

        CommerceEntity Find(string key);
    }
}
