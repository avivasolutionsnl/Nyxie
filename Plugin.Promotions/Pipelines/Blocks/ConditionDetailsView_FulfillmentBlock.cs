using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Promethium.Plugin.Promotions.Extensions;

namespace Promethium.Plugin.Promotions.Pipelines.Blocks
{
    public class ConditionDetailsView_FulfillmentBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly GetFulfillmentMethodsCommand _getCommand;

        public ConditionDetailsView_FulfillmentBlock(GetFulfillmentMethodsCommand getCommand)
        {
            _getCommand = getCommand;
        }

        public override Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull(arg.Name + ": The argument cannot be null");

            var condition = arg.Properties.FirstOrDefault(p => p.Name.EqualsOrdinalIgnoreCase("Condition"));
            if (condition == null || !condition.RawValue.ToString().StartsWith("Promethium_") || !condition.RawValue.ToString().EndsWith("FulfillmentCondition"))
            {
                return Task.FromResult(arg);
            }

            var categorySelection = arg.Properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("Promethium_SpecificFulfillment"));
            if (categorySelection != null)
            {
                var fulfillmentMethods = _getCommand.Process(context.CommerceContext).Result;
                var options = fulfillmentMethods.Select(x => new Selection { DisplayName = x.DisplayName, Name = x.Name });

                var policy = new AvailableSelectionsPolicy(options);
                categorySelection.Policies.Add(policy);
            }

            return Task.FromResult(arg);
        }
    }
}