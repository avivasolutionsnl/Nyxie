﻿using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
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
        private readonly GetCatalogsCommand _getCatalogsCommand;
        private readonly GetCategoriesCommand _getCategoriesCommand;

        public ConditionDetailsView_CategoryBlock(GetCatalogsCommand getCatalogsCommand, GetCategoriesCommand getCategoriesCommand)
        {
            _getCatalogsCommand = getCatalogsCommand;
            _getCategoriesCommand = getCategoriesCommand;
        }

        public override Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull(arg.Name + ": The argument cannot be null");

            var condition = arg.Properties.FirstOrDefault(p => p.Name.Equals("Condition", StringComparison.OrdinalIgnoreCase));
            if (condition == null || !condition.RawValue.ToString().StartsWith("Promethium_") || !condition.RawValue.ToString().EndsWith("CategoryCondition"))
            {
                return Task.FromResult(arg);
            }

            var categorySelection = arg.Properties.FirstOrDefault(x => x.Name.Equals("Promethium_SpecificCategory", StringComparison.OrdinalIgnoreCase));
            if (categorySelection != null)
            {
                var catalogs = _getCatalogsCommand.Process(context.CommerceContext).Result;

                var catalog = catalogs.First();
                var categories = _getCategoriesCommand.Process(context.CommerceContext, catalog.Name).Result;

                var allCategories = categories.Where(x => x.ParentCategoryList != null).ToList();

                var selectOptions = new List<Selection>();

                var topCategories = catalog.ChildrenCategoryList.Split('|');
                foreach (var topCategory in topCategories)
                {
                    GetCategories(topCategory, allCategories, "", ref selectOptions);
                }

                var policy = new AvailableSelectionsPolicy(selectOptions);

                categorySelection.Policies.Add(policy);
            }

            return Task.FromResult(arg);
        }

        private void GetCategories(string parentCategoryId, List<Category> allCategories, string displayName, ref List<Selection> selectOptions)
        {
            var categories = allCategories.Where(x => x.ParentCategoryList.Equals(parentCategoryId)).ToList();
            if (categories.Any())
            {
                categories = categories.OrderBy(x => x.Name).ToList();
                foreach (var category in categories)
                {
                    var optionDisplayName = $"{displayName}/{category.Name}";
                    selectOptions.Add(new Selection { DisplayName = optionDisplayName, Name = category.SitecoreId });

                    GetCategories(category.SitecoreId, allCategories, optionDisplayName, ref selectOptions);
                }
            }
        }
    }
}