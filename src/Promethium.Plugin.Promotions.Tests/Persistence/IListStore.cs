using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sitecore.Commerce.Core;

namespace Promethium.Plugin.Promotions.Tests.Persistence
{
    public interface IListStore
    {
        void Add(string list, CommerceEntity entity);
        IEnumerable<CommerceEntity> GetEntitiesInList(string list);
    }
}
