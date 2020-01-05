using System.Collections.Concurrent;

using Sitecore.Commerce.Core;

namespace Hotcakes.Plugin.Promotions.Tests.Persistence
{
    public class InMemoryStore : IStore
    {
        public ConcurrentDictionary<string, CommerceEntity> Entities = new ConcurrentDictionary<string, CommerceEntity>();

        public void Add(CommerceEntity entity)
        {
            Entities.TryAdd(entity.Id, entity);
        }

        public CommerceEntity Find(string key)
        {
            CommerceEntity entity;

            if (Entities.TryGetValue(key, out entity))
                return entity;

            return null;
        }
    }
}
