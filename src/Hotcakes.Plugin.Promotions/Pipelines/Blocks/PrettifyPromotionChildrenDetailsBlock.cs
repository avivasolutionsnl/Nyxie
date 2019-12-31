using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Hotcakes.Plugin.Promotions.Extensions;
using Hotcakes.Plugin.Promotions.Properties;
using Hotcakes.Plugin.Promotions.Resolvers;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Hotcakes.Plugin.Promotions.Pipelines.Blocks
{
    public class PrettifyPromotionChildrenDetailsBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly GetSellableItemCommand _getItemCommand;
        private readonly CategoryPathResolver categoryPathResolver;

        public PrettifyPromotionChildrenDetailsBlock(GetSellableItemCommand getItemCommand, CategoryPathResolver categoryPathResolver)
        {
            _getItemCommand = getItemCommand;
            this.categoryPathResolver = categoryPathResolver;
        }

        public override async Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull(arg.Name + ": The argument cannot be null");

            var type = arg.Properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("Type"));
            if (type == null || !type.RawValue.ToString().EqualsOrdinalIgnoreCase("Promotion"))
            {
                return arg;
            }

            var commerceContext = context.CommerceContext;

            await PrettifyChildDetails(arg, "Qualifications", commerceContext);
            await PrettifyChildDetails(arg, "Benefits", commerceContext);

            return arg;
        }

        private async Task PrettifyChildDetails(EntityView arg, string childPartName, CommerceContext commerceContext)
        {
            if (arg.ChildViews.Any(x => x.Name.EqualsOrdinalIgnoreCase(childPartName)))
            {
                var childPart = arg.ChildViews
                    .OfType<EntityView>()
                    .First(x => x.Name.EqualsOrdinalIgnoreCase(childPartName));
                var childrenToProcess = childPart.ChildViews
                    .OfType<EntityView>()
                    .Where(x => x.Properties.Any(y => y.Name.StartsWith("Hc_")))
                    .ToList();

                if (childrenToProcess.Any())
                {
                    foreach (var child in childrenToProcess)
                    {
                        await PrettifyChild(child, commerceContext);
                    }
                }
            }
        }

        private async Task PrettifyChild(EntityView entity, CommerceContext commerceContext)
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

                    var variableValue = await PrettifyVariableValue(variable, entity.Properties, commerceContext);
                    fullEntity.Value = fullEntity.Value.Replace(match.Value, $"<strong>{variableValue}</strong>");
                }
            }

            fullEntity.Value = fullEntity.Value.Replace("$", commerceContext.CurrentCurrency());

            // A quick and dirty resolution to let the content go over multiple lines.
            // Add extra div's with specific default classes because adding custom style is stripped by Angular.
            // The .dropdown-header class is for overwriting the white-space: nowrap, the .col-form-legend is for setting the font-size back to normal and .p-0 for removing the padding added by these classes
            fullEntity.Value = $"<div class='dropdown-header p-0 border-0'><div class='col-form-legend p-0'>{fullEntity.Value}</div></div>";
        }

        private async Task<string> PrettifyVariableValue(ViewProperty variable, List<ViewProperty> properties, CommerceContext commerceContext)
        {
            switch (variable.Name)
            {
                case "Hc_BasicStringCompare":
                case "Hc_Compares":
                case "Hc_Operator":
                    return variable.Value.PrettifyOperatorName();

                case "Hc_SpecificCategory":
                    return await PrettifyCategory(variable.Value, properties, commerceContext);

                case "Hc_Date":
                    if (DateTimeOffset.TryParse(variable.Value, out DateTimeOffset date))
                    {
                        return date.LocalDateTime.ToString("d MMM yyyy HH:mm");
                    }

                    return variable.Value;

                case "Hc_ApplyActionTo":
                    return ApplicationOrder.Parse(variable.Value).DisplayName;

                case "TargetItemId":
                    return await PrettifyProduct(variable.Value, commerceContext);

                default:
                    return variable.Value;
            }
        }

        private async Task<string> PrettifyCategory(string categoryCommerceId, List<ViewProperty> properties, CommerceContext commerceContext)
        {
            var output = await categoryPathResolver.GetCategoryPath(commerceContext, categoryCommerceId);

            var includeSubCategories = properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("Hc_IncludeSubCategories"));
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

        private async Task<string> PrettifyProduct(string input, CommerceContext commerceContext)
        {
            var sellableItem = await _getItemCommand.Process(commerceContext, input, false);
            return sellableItem != null ? sellableItem.DisplayName : input;
        }
    }
}