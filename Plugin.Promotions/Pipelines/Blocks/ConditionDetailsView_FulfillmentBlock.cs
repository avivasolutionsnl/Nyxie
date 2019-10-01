using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
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

        public override Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull(arg.Name + ": The argument cannot be null");

            var entityViewArgument = context.CommerceContext.GetObject<EntityViewArgument>();
            if (string.IsNullOrEmpty(entityViewArgument?.ViewName) || !entityViewArgument.ViewName.Equals(
                    context.GetPolicy<KnownPromotionsViewsPolicy>().QualificationDetails,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(arg);
            }

            var knownPolicies = context.GetPolicy<KnownPromotionsActionsPolicy>();
            var editQualification = entityViewArgument.ForAction.Equals(knownPolicies.EditQualification, StringComparison.OrdinalIgnoreCase) ||
                                    entityViewArgument.ForAction.Equals(knownPolicies.AddQualification, StringComparison.OrdinalIgnoreCase);
            if (!(entityViewArgument.Entity is Promotion) || !editQualification)
            {
                return Task.FromResult(arg);
            }

            var condition = arg.Properties.FirstOrDefault(p => p.Name.Equals("Condition", StringComparison.OrdinalIgnoreCase));
            if (condition == null || !condition.Value.StartsWith("Promethium_") || !condition.Value.EndsWith("FulfillmentCondition"))
            {
                return Task.FromResult(arg);
            }

            var categorySelection = arg.Properties.FirstOrDefault(x => x.Name.Equals("SpecificFulfillment", StringComparison.OrdinalIgnoreCase));
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