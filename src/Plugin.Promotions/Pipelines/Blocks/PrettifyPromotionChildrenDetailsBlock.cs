using Promethium.Plugin.Promotions.Classes;
using Promethium.Plugin.Promotions.Extensions;
using Promethium.Plugin.Promotions.Properties;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Management;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Promethium.Plugin.Promotions.Factory;

namespace Promethium.Plugin.Promotions.Pipelines.Blocks
{
    public class PrettifyPromotionChildrenDetailsBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly GetCategoryCommand _getCategoryCommand;
        private readonly GetSellableItemCommand _getItemCommand;
        private readonly SitecoreConnectionManager _manager;
        private CommerceContext _commerceContext;
        private CategoryFactory _categoryFactory;

        public PrettifyPromotionChildrenDetailsBlock(GetCategoryCommand getCategoryCommand, GetSellableItemCommand getItemCommand, SitecoreConnectionManager manager)
        {
            _getCategoryCommand = getCategoryCommand;
            _getItemCommand = getItemCommand;
            _manager = manager;
        }

        public override async Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            _commerceContext = context.CommerceContext;
            _categoryFactory = new CategoryFactory(_commerceContext, _manager, _getCategoryCommand);

            Condition.Requires(arg).IsNotNull(arg.Name + ": The argument cannot be null");

            var type = arg.Properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("Type"));
            if (type == null || !type.RawValue.ToString().EqualsOrdinalIgnoreCase("Promotion"))
            {
                return arg;
            }

            await PrettifyChildDetails(arg, "Qualifications");
            await PrettifyChildDetails(arg, "Benefits");

            return arg;
        }

        private async Task PrettifyChildDetails(EntityView arg, string childPartName)
        {
            if (arg.ChildViews.Any(x => x.Name.EqualsOrdinalIgnoreCase(childPartName)))
            {
                var childPart = arg.ChildViews
                    .OfType<EntityView>()
                    .First(x => x.Name.EqualsOrdinalIgnoreCase(childPartName));
                var childrenToProcess = childPart.ChildViews
                    .OfType<EntityView>()
                    .Where(x => x.Properties.Any(y => y.Name.StartsWith("Pm_")))
                    .ToList();

                if (childrenToProcess.Any())
                {
                    foreach (var child in childrenToProcess)
                    {
                        await PrettifyChild(child);
                    }
                }
            }
        }

        private async Task PrettifyChild(EntityView entity)
        {
            var originalEntity = entity.Properties.First(x => x.Name.EqualsOrdinalIgnoreCase("Condition") || x.Name.EqualsOrdinalIgnoreCase("Action"));
            originalEntity.IsHidden = true;

            var fullEntity = new ViewProperty
            {
                DisplayName = originalEntity.DisplayName,
                IsHidden = false,
                IsReadOnly = originalEntity.IsReadOnly,
                IsRequired = originalEntity.IsRequired,
                Name = $"Full{originalEntity.Name}",
                OriginalType = "Html",
                Policies = originalEntity.Policies,
                RawValue = originalEntity.RawValue,
                Value = originalEntity.Value
            };

            entity.Properties.Insert(entity.Properties.IndexOf(originalEntity), fullEntity);

            //Add the condition operator if exists at the begin of the condition
            var conditionOperator = entity.Properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("ConditionOperator"));
            if (conditionOperator != null)
            {
                conditionOperator.IsHidden = true;
                fullEntity.Value = $"{conditionOperator.Value} {fullEntity.Value}";
            }

            //Replace all the variables within [] in the condition with the values
            var regex = new Regex("\\[(.*?)\\]");
            var matches = regex.Matches(fullEntity.Value);
            foreach (Match match in matches)
            {
                var variableName = match.Groups[1].ToString();
                var variable = entity.Properties.FirstOrDefault(x => x.DisplayName.EqualsOrdinalIgnoreCase(variableName));

                if (variable == null && variableName.EqualsOrdinalIgnoreCase("Gift"))
                {
                    variable = entity.Properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("TargetItemId"));
                }

                if (variable != null)
                {
                    variable.IsHidden = true;

                    var variableValue = await PrettifyVariableValue(variable, entity.Properties);
                    fullEntity.Value = fullEntity.Value.Replace(match.Value, $"<strong>{variableValue}</strong>");
                }
            }

            fullEntity.Value = fullEntity.Value.Replace("$", _commerceContext.CurrentCurrency());

            // A quick and dirty resolution to let the content go over multiple lines.
            // Add extra div's with specific default classes because adding custom style is stripped by Angular.
            // The .dropdown-header class is for overwriting the white-space: nowrap, the .col-form-legend is for setting the font-size back to normal and .p-0 for removing the padding added by these classes
            fullEntity.Value = $"<div class='dropdown-header p-0 border-0'><div class='col-form-legend p-0'>{fullEntity.Value}</div></div>";
        }

        private async Task<string> PrettifyVariableValue(ViewProperty variable, List<ViewProperty> properties)
        {
            switch (variable.Name)
            {
                case "Pm_BasicStringCompare":
                case "Pm_Compares":
                case "Pm_Operator":
                    return variable.Value.PrettifyOperatorName();

                case "Pm_SpecificCategory":
                    return await PrettifyCategory(variable.Value, properties);

                case "Pm_Date":
                    if (DateTimeOffset.TryParse(variable.Value, out DateTimeOffset date))
                    {
                        return date.LocalDateTime.ToString("d MMM yyyy HH:mm");
                    }

                    return variable.Value;

                case "Pm_ApplyActionTo":
                    return ActionProductOrdener.Options.First(x => x.Name == variable.Value).DisplayName;

                case "TargetItemId":
                    return await PrettifyProduct(variable.Value);

                default:
                    return variable.Value;
            }
        }

        private async Task<string> PrettifyCategory(string categoryCommerceId, List<ViewProperty> properties)
        {
            var output = await _categoryFactory.GetCategoryPath(categoryCommerceId);

            var includeSubCategories = properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("Pm_IncludeSubCategories"));
            if (includeSubCategories != null)
            {
                includeSubCategories.IsHidden = true;

                if (bool.TryParse(includeSubCategories.Value, out bool value) && value)
                {
                    output += " " + Resources.IncludingSubCategories;
                }
            }

            return output;
        }

        private async Task<string> PrettifyProduct(string input)
        {
            var sellableItem = await _getItemCommand.Process(_commerceContext, input, false);
            return sellableItem != null ? sellableItem.DisplayName : input;
        }
    }
}