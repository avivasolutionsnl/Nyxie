using Xunit;

namespace Promethium.Plugin.Promotions.Tests
{
    [CollectionDefinition("Engine collection")]
    public class EngineCollection : ICollectionFixture<EngineFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
