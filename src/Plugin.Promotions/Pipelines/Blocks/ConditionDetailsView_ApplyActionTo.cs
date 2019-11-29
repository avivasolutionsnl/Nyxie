using Promethium.Plugin.Promotions.Classes;
using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System.Linq;
using System.Threading.Tasks;

namespace Promethium.Plugin.Promotions.Pipelines.Blocks
{
    public class ConditionDetailsView_ApplyActionTo : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        public override Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull(arg.Name + ": The argument cannot be null");

            var action = arg.Properties.FirstOrDefault(p => p.Name.EqualsOrdinalIgnoreCase("Action"));
            if (action == null || !action.RawValue.ToString().StartsWith("Pm_") || !action.RawValue.ToString().EndsWith("Action"))
            {
                return Task.FromResult(arg);
            }

            var applyActionTo = arg.Properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("Pm_ApplyActionTo"));
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