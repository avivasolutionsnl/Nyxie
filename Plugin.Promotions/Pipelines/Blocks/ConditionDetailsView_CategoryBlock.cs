using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Promethium.Plugin.Promotions.Pipelines.Blocks
{
    public class ConditionDetailsView_CategoryBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
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
            if (condition == null || !condition.RawValue.ToString().StartsWith("Promethium_") || !condition.RawValue.ToString().EndsWith("CategoryCondition"))
            {
                return Task.FromResult(arg);
            }

            var categorySelection = arg.Properties.FirstOrDefault(x => x.Name.Equals("SpecificCategory", StringComparison.OrdinalIgnoreCase));
            if (categorySelection != null)
            {
                var policy = new AvailableSelectionsPolicy();
                policy.AllowMultiSelect = false;
                policy.List = new List<Selection>
                {
                    new Selection {DisplayName = "/Appliances", IsDefault = false, Name = "/Appliances"},
                    new Selection {DisplayName = "/Appliances/Laundry", IsDefault = false, Name = "/Appliances/Laundry"},
                    new Selection {DisplayName = "/Appliances/Microwaves", IsDefault = false, Name = "/Appliances/Microwaves"},
                    new Selection {DisplayName = "/Appliances/Ranges", IsDefault = false, Name = "/Appliances/Ranges"},
                    new Selection {DisplayName = "/Appliances/Refrigerators", IsDefault = false, Name = "/Appliances/Refrigerators"},
                    new Selection {DisplayName = "/Appliances/Small Appliances", IsDefault = false, Name = "/Appliances/Small Appliances"},
                    new Selection {DisplayName = "/Appliances/Warranties and Installations", IsDefault = false, Name = "/Appliances/Warranties and Installations"},
                    new Selection {DisplayName = "/Audio", IsDefault = false, Name = "/Audio"},
                    new Selection {DisplayName = "/Cameras", IsDefault = false, Name = "/Cameras"},
                    new Selection {DisplayName = "/Computers and Tablets", IsDefault = false, Name = "/Computers and Tablets"},
                    new Selection {DisplayName = "/Connected home", IsDefault = false, Name = "/Connected home"},
                    new Selection {DisplayName = "/eGift Cards and Gift Wrapping", IsDefault = false, Name = "/eGift Cards and Gift Wrapping"},
                    new Selection {DisplayName = "/Gaming", IsDefault = false, Name = "/Gaming"},
                    new Selection {DisplayName = "/Health, Beauty and Fitness", IsDefault = false, Name = "/Health, Beauty and Fitness"},
                    new Selection {DisplayName = "/Home Theater", IsDefault = false, Name = "/Home Theater"},
                    new Selection {DisplayName = "/Phones", IsDefault = false, Name = "/Phones"},
                    new Selection {DisplayName = "/Televisions", IsDefault = false, Name = "/Televisions"},
                };

                categorySelection.Policies.Add(policy);
            }

            return Task.FromResult(arg);
        }
    }
}