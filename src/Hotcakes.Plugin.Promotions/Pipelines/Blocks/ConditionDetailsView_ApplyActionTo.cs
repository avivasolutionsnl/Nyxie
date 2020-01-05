using System.Linq;
using System.Threading.Tasks;

using Hotcakes.Plugin.Promotions.Extensions;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Hotcakes.Plugin.Promotions.Pipelines.Blocks
{
    public class ConditionDetailsView_ApplyActionTo : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        public override Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull(arg.Name + ": The argument cannot be null");

            ViewProperty action = arg.Properties.FirstOrDefault(p => p.Name.EqualsOrdinalIgnoreCase("Action"));
            if (action == null || !action.RawValue.ToString().StartsWith("Hc_") || !action.RawValue.ToString().EndsWith("Action"))
                return Task.FromResult(arg);

            ViewProperty applyActionTo = arg.Properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("Hc_ApplyActionTo"));
            applyActionTo?.Policies.Add(new AvailableSelectionsPolicy(
                ApplicationOrder.All.Select(x => new Selection
                {
                    Name = x.Name,
                    DisplayName = x.DisplayName
                })));

            return Task.FromResult(arg);
        }
    }
}
