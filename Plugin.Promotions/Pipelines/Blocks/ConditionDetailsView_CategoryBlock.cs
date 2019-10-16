using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Management;
using Sitecore.Commerce.Plugin.Search;
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
        private readonly GetCategoryCommand _getCategoryCommand;

        public ConditionDetailsView_CategoryBlock(GetCategoryCommand getCategoryCommand)
        {
            _getCategoryCommand = getCategoryCommand;
        }

        public override async Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull(arg.Name + ": The argument cannot be null");

            var entity = arg.Properties.FirstOrDefault(p => p.Name.EqualsOrdinalIgnoreCase("Condition") || p.Name.EqualsOrdinalIgnoreCase("Action"));
            if (entity == null || !entity.RawValue.ToString().StartsWith("Pm_") || !entity.RawValue.ToString().Contains("InCategory"))
            {
                return arg;
            }

            var categorySelection = arg.Properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("Pm_SpecificCategory"));
            if (categorySelection == null)
            {
                return arg;
            }

            var policyByType = SearchScopePolicy.GetPolicyByType(context.CommerceContext, context.CommerceContext.Environment, typeof(Category));
            if (policyByType != null)
            {
                var policy = new Policy()
                {
                    PolicyId = "EntityType",
                    Models = new List<Model> { new Model { Name = "Category" } }
                };
                categorySelection.UiType = "Autocomplete";
                categorySelection.Policies.Add(policy);
                categorySelection.Policies.Add(policyByType);

                if (categorySelection.RawValue != null &&
                    !string.IsNullOrEmpty(categorySelection.RawValue.ToString()) &&
                    categorySelection.RawValue.ToString().IndexOf("-Category-", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    var readOnlyProp = new ViewProperty
                    {
                        DisplayName = $"Full category path of '{categorySelection.RawValue}'",
                        IsHidden = false,
                        IsReadOnly = true,
                        Name = "FullCategoryPath",
                        IsRequired = false,
                        OriginalType = "System.String",
                        Value = await PrettifyCategory(context.CommerceContext, categorySelection.RawValue.ToString(), _getCategoryCommand),
                    };

                    arg.Properties.Insert(arg.Properties.IndexOf(categorySelection) + 1, readOnlyProp);
                }
            }

            return arg;
        }

        private static async Task<string> PrettifyCategory(CommerceContext commerceContext, string input, GetCategoryCommand getCategoryCommand)
        {
            var category = await getCategoryCommand.Process(commerceContext, input);
            if (category == null)
            {
                return input;
            }

            var output = $"/{category.DisplayName}";

            var manager = new SitecoreConnectionManager();
            var parent = await manager.GetItemByIdAsync(commerceContext, category.ParentCategoryList);
            while (parent != null)
            {
                if (parent["ParentCategoryList"] == null ||
                    string.IsNullOrWhiteSpace(parent["ParentCategoryList"].ToString()))
                {
                    break;
                }

                output = $"/{parent["DisplayName"]}{output}";

                parent = await manager.GetItemByIdAsync(commerceContext, parent["ParentCategoryList"].ToString());
            }

            return output;
        }
    }
}