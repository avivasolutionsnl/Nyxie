using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Promethium.Plugin.Promotions.Pipelines.Blocks
{
    public class PrettifySelectOptionsBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        public override Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull(arg.Name + ": The argument cannot be null");

            var condition = arg.Properties.FirstOrDefault(p => p.Name.EqualsOrdinalIgnoreCase("Condition"));
            if (condition == null || !condition.RawValue.ToString().StartsWith("Promethium_") || !condition.RawValue.ToString().EndsWith("Condition"))
            {
                return Task.FromResult(arg);
            }

            foreach (var property in arg.Properties)
            {
                if (property.Policies == null || !property.Policies.Any()) continue;

                foreach (var policy in property.Policies)
                {
                    if (!(policy is AvailableSelectionsPolicy selectionsPolicy)) continue;

                    foreach (var selection in selectionsPolicy.List)
                    {
                        selection.DisplayName = selection.DisplayName.PrettifyOperatorName();
                    }
                }
            }

            return Task.FromResult(arg);
        }
    }
}
