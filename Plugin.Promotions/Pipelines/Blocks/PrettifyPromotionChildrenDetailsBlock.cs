using Promethium.Plugin.Promotions.Classes;
using Promethium.Plugin.Promotions.Extensions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Promethium.Plugin.Promotions.Properties;

namespace Promethium.Plugin.Promotions.Pipelines.Blocks
{
    public class PrettifyPromotionChildrenDetailsBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly GetCatalogsCommand _getCatalogsCommand;
        private readonly GetCategoriesCommand _getCategoriesCommand;
        private readonly GetSellableItemCommand _getItemCommand;
        private CommerceContext _commerceContext;

        private Catalog _catalog;
        private List<Category> _categories;
        private List<Category> Categories
        {
            get
            {
                if (_categories == null)
                {
                    var catalogs = _getCatalogsCommand.Process(_commerceContext).Result;

                    _catalog = catalogs.FirstOrDefault(); //Make the assumption that there is only 1 catalog
                    if (_catalog != null)
                    {
                        _categories = _getCategoriesCommand.Process(_commerceContext, _catalog.Name).Result.ToList();
                    }
                }

                return _categories;
            }
        }

        public PrettifyPromotionChildrenDetailsBlock(GetCatalogsCommand getCatalogsCommand, GetCategoriesCommand getCategoriesCommand, GetSellableItemCommand getItemCommand)
        {
            _getCatalogsCommand = getCatalogsCommand;
            _getCategoriesCommand = getCategoriesCommand;
            _getItemCommand = getItemCommand;
        }

        public override Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            _commerceContext = context.CommerceContext;

            Condition.Requires(arg).IsNotNull(arg.Name + ": The argument cannot be null");

            var type = arg.Properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("Type"));
            if (type == null || !type.RawValue.ToString().EqualsOrdinalIgnoreCase("Promotion"))
            {
                return Task.FromResult(arg);
            }

            PrettifyChildDetails(arg, "Qualifications");
            PrettifyChildDetails(arg, "Benefits");

            return Task.FromResult(arg);
        }

        private void PrettifyChildDetails(EntityView arg, string childPartName)
        {
            if (arg.ChildViews.Any(x => x.Name.EqualsOrdinalIgnoreCase(childPartName)))
            {
                var childPart = arg.ChildViews
                    .Select(x => (EntityView) x)
                    .First(x => x.Name.EqualsOrdinalIgnoreCase(childPartName));
                var childrenToProcess = childPart.ChildViews
                    .Select(x => (EntityView) x)
                    .Where(x => x.Properties.Any(y => y.Name.StartsWith("Promethium_")))
                    .ToList();

                if (childrenToProcess.Any())
                {
                    foreach (var child in childrenToProcess)
                    {
                        PrettifyChild(child);
                    }
                }
            }
        }

        private void PrettifyChild(EntityView entity)
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
                RawValue =  originalEntity.RawValue,
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

                    var variableValue = PrettifyVariableValue(variable, entity.Properties);
                    fullEntity.Value = fullEntity.Value.Replace(match.Value, $"<strong>{variableValue}</strong>");
                }
            }

            fullEntity.Value = fullEntity.Value.Replace("$", _commerceContext.CurrentCurrency());

            // A quick and dirty resolution to let the content go over multiple lines.
            // Add extra div's with specific default classes because adding custom style is stripped by Angular.
            // The .dropdown-header class is for overwriting the white-space: nowrap, the .col-form-legend is for setting the font-size back to normal and .p-0 for removing the padding added by these classes
            fullEntity.Value = $"<div class='dropdown-header p-0 border-0'><div class='col-form-legend p-0'>{fullEntity.Value}</div></div>";
        }

        private string PrettifyVariableValue(ViewProperty variable, List<ViewProperty> properties)
        {
            switch (variable.Name)
            {
                case "Promethium_BasicStringCompare":
                case "Promethium_Compares":
                case "Promethium_Operator":
                    return variable.Value.PrettifyOperatorName();

                case "Promethium_SpecificCategory":
                    return PrettifyCategory(variable.Value, properties);

                case "Promethium_Date":
                    if (DateTimeOffset.TryParse(variable.Value, out DateTimeOffset date))
                    {
                        return date.LocalDateTime.ToString("d MMM yyyy HH:mm");
                    }

                    return variable.Value;

                case "Promethium_ApplyActionTo":
                    return ActionProductOrdener.Options.First(x => x.Name == variable.Value).DisplayName;

                case "TargetItemId":
                    return PrettifyProduct(variable.Value);

                default:
                    return variable.Value;
            }
        }

        private string PrettifyCategory(string input, List<ViewProperty> properties)
        {
            var output = "";
            var category = Categories.FirstOrDefault(x => x.SitecoreId == input);
            while (category != null)
            {
                if (category.ParentCategoryList == null)
                {
                    break;
                }

                output = $"/{category.DisplayName}{output}";

                category = Categories.FirstOrDefault(x => x.SitecoreId == category.ParentCategoryList);
            }

            var includeSubCategories = properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("Promethium_IncludeSubCategories"));
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

        private string PrettifyProduct(string input)
        {
            var sellableItem = _getItemCommand.Process(_commerceContext, input, false).Result;
            if (sellableItem != null)
            {
                return sellableItem.DisplayName;
            }

            return input;
        }
    }
}
