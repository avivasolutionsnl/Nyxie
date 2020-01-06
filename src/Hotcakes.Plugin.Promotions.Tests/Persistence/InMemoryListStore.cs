using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Sitecore.Commerce.Core;

namespace Hotcakes.Plugin.Promotions.Tests.Persistence
{
    public class InMemoryListStore : IListStore
    {
        public ConcurrentDictionary<string, ConcurrentBag<CommerceEntity>> Lists =
            new ConcurrentDictionary<string, ConcurrentBag<CommerceEntity>>();

        public void Add(string list, CommerceEntity entity)
        {
            Lists.GetOrAdd(list, new ConcurrentBag<CommerceEntity>()).Add(entity);
        }

        public IEnumerable<CommerceEntity> GetEntitiesInList(string list)
        {
            ConcurrentBag<CommerceEntity> entities;

            if (!Lists.TryGetValue(list, out entities))
                return Enumerable.Empty<CommerceEntity>();

            return entities.ToArray();
        }
    }
}
