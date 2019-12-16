using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Promethium.Plugin.Promotions.Tests.Persistence.Pipelines.Blocks
{
    [PipelineDisplayName("Isocore.Persistence.FindEntitiesInListBlock")]
    public class FindEntitiesInListBlock : PipelineBlock<FindEntitiesInListArgument, FindEntitiesInListArgument, CommercePipelineExecutionContext>
    {
        private readonly IListStore listStore;

        public FindEntitiesInListBlock(IListStore listStore)
        {
            this.listStore = listStore;
        }   

        public override Task<FindEntitiesInListArgument> Run(FindEntitiesInListArgument arg, CommercePipelineExecutionContext context)
        {
            var entities = listStore.GetEntitiesInList(arg.ListName);

            arg.List = new CommerceList<CommerceEntity>()
            {
                Name = arg.ListName,
                DisplayName = arg.ListName,
                CurrentPage = 1,
                TotalItemCount = entities.Count(),
                Items = entities.ToList()
            };

            return Task.FromResult(arg);
        }
    }
}
