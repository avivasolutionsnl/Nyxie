using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Framework.Pipelines;

namespace Promethium.Plugin.Promotions.Tests.Persistence.Pipelines.Blocks
{
    [PipelineDisplayName("Promotions.block.FilterPromotionsByBookAssociatedCatalogs")]
    public class LoggingBlock<T> : PipelineBlock<IEnumerable<Promotion>, IEnumerable<Promotion>, CommercePipelineExecutionContext> where T : PipelineBlock<IEnumerable<Promotion>, IEnumerable<Promotion>, CommercePipelineExecutionContext>
    {
        private readonly T wrapped;

        public LoggingBlock(T wrapped)
          : base((string)null)
        {
            this.wrapped = wrapped;
        }

        public override async Task<IEnumerable<Promotion>> Run(
          IEnumerable<Promotion> arg,
          CommercePipelineExecutionContext context)
        {
            IEnumerable<Promotion> result = await wrapped.Run(arg, context);

            context.Logger.LogDebug($"{result.Count()} promotions survived promotion block: {typeof(T).FullName}");

            return result;
        }
    }
}
