using System.Linq;
using System.Threading.Tasks;

using Hotcakes.Plugin.Promotions.Extensions;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Hotcakes.Plugin.Promotions.Pipelines.Blocks
{
    public class ConditionDetailsView_BasicStringCompareBlock : PipelineBlock<EntityView, EntityView,
        CommercePipelineExecutionContext>
    {
        public override Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull(arg.Name + ": The argument cannot be null");

            ViewProperty condition = arg.Properties.FirstOrDefault(p => p.Name.EqualsOrdinalIgnoreCase("Condition"));
            if (condition == null || !condition.RawValue.ToString().StartsWith("Hc_") ||
                !condition.RawValue.ToString().EndsWith("Condition"))
                return Task.FromResult(arg);

            ViewProperty stringComparer =
                arg.Properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("Hc_BasicStringCompare"));
            stringComparer?.Policies.Add(new AvailableSelectionsPolicy(BasicStringComparer.Options));

            return Task.FromResult(arg);
        }
    }
}
