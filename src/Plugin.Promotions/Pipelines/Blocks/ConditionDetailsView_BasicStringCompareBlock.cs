using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System.Linq;
using System.Threading.Tasks;

namespace Promethium.Plugin.Promotions.Pipelines.Blocks
{
    public class ConditionDetailsView_BasicStringCompareBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        public override Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull(arg.Name + ": The argument cannot be null");

            var condition = arg.Properties.FirstOrDefault(p => p.Name.EqualsOrdinalIgnoreCase("Condition"));
            if (condition == null || !condition.RawValue.ToString().StartsWith("Pm_") || !condition.RawValue.ToString().EndsWith("Condition"))
            {
                return Task.FromResult(arg);
            }

            var stringComparer = arg.Properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("Pm_BasicStringCompare"));
            stringComparer?.Policies.Add(new AvailableSelectionsPolicy(BasicStringComparer.Options));

            return Task.FromResult(arg);
        }
    }
}