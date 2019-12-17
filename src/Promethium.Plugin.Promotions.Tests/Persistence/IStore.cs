using Sitecore.Commerce.Core;

namespace Promethium.Plugin.Promotions.Tests.Persistence
{
    public interface IStore
    {
        void Add(CommerceEntity entity);

        CommerceEntity Find(string key);
    }
}
