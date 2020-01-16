using System;
using System.Threading.Tasks;

using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Nyxie.Plugin.Promotions.Tests.Persistence.Pipelines.Blocks
{
    [PipelineDisplayName("InMemory.Persistence.FindEntityBlock")]
    public class FindEntityBlock : PipelineBlock<FindEntityArgument, CommerceEntity, CommercePipelineExecutionContext>
    {
        private readonly IStore store;

        public FindEntityBlock(IStore store)
        {
            this.store = store;
        }

        public override Task<CommerceEntity> Run(FindEntityArgument arg, CommercePipelineExecutionContext context)
        {
            CommerceEntity entity = store.Find(arg.EntityId);

            if (entity == null && arg.ShouldCreate)
                entity = Activator.CreateInstance(arg.EntityType) as CommerceEntity;

            return Task.FromResult(entity);
        }
    }
}
