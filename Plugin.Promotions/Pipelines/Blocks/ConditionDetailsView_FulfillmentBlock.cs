using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System.Linq;
using System.Threading.Tasks;

namespace Promethium.Plugin.Promotions.Pipelines.Blocks
{
    public class ConditionDetailsView_FulfillmentBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly GetFulfillmentMethodsCommand _getCommand;

        public ConditionDetailsView_FulfillmentBlock(GetFulfillmentMethodsCommand getCommand)
        {
            _getCommand = getCommand;
        }

        public override async Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull(arg.Name + ": The argument cannot be null");

            var condition = arg.Properties.FirstOrDefault(p => p.Name.EqualsOrdinalIgnoreCase("Condition"));
            if (condition == null || !condition.RawValue.ToString().StartsWith("Pm_") || !condition.RawValue.ToString().EndsWith("FulfillmentCondition"))
            {
                return arg;
            }

            var fulfillmentSelection = arg.Properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("Pm_SpecificFulfillment"));
            if (fulfillmentSelection != null)
            {
                var fulfillmentMethods = await _getCommand.Process(context.CommerceContext);
                var options = fulfillmentMethods.Select(x => new Selection { DisplayName = x.DisplayName, Name = x.Name });

                var policy = new AvailableSelectionsPolicy(options);
                fulfillmentSelection.Policies.Add(policy);
            }

            return arg;
        }
    }
}