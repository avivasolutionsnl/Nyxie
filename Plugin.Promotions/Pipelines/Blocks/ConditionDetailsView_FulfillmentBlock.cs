using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Commerce.Plugin.Fulfillment;

namespace Promethium.Plugin.Promotions.Pipelines.Blocks
{
    public class ConditionDetailsView_FulfillmentBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private GetFulfillmentMethodsCommand _getCommand;

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
            if (condition == null || !condition.RawValue.ToString().StartsWith("Promethium_") || !condition.RawValue.ToString().EndsWith("FulfillmentCondition"))
            {
                return Task.FromResult(arg);
            }

            var categorySelection = arg.Properties.FirstOrDefault(x => x.Name.Equals("SpecificFulfillment", StringComparison.OrdinalIgnoreCase));
            if (categorySelection != null)
            {
                var fulfillmentMethods = _getCommand.Process(context.CommerceContext).Result;

                var policy = new AvailableSelectionsPolicy
                {
                    AllowMultiSelect = false,
                    List = fulfillmentMethods.Select(x => new Selection { DisplayName = x.DisplayName, Name = x.Name }).ToList()
                };

                categorySelection.Policies.Add(policy);
            }

            return Task.FromResult(arg);
        }
    }
}

// /sitecore/Commerce/Commerce Control Panel/Shared Settings/Fulfillment Options
// /sitecore/templates/CommerceConnect/Sitecore Commerce/Commerce Control Panel/Shared Settings/Fulfillment/Fulfillment Option
// {7CC3185B-6DEF-4D8E-BF12-D981DFD2B614}