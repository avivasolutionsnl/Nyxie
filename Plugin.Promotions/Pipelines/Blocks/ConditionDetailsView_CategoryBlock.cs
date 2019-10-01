using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Commerce.Plugin.Catalog;

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
            if (condition == null || !condition.Value.StartsWith("Promethium_") || !condition.Value.EndsWith("CategoryCondition"))
            {
                return Task.FromResult(arg);
            }

            var categorySelection = arg.Properties.FirstOrDefault(x => x.Name.Equals("SpecificCategory", StringComparison.OrdinalIgnoreCase));
            if (categorySelection != null)
            {
                var catalogs = _getCatalogsCommand.Process(context.CommerceContext).Result;

                var catalog = catalogs.First();
                var categories = _getCategoriesCommand.Process(context.CommerceContext, catalog.Name).Result;

                var allCategories = categories.Where(x => x.ParentCategoryList != null)
                    .Select(x => new Category {
                        Name = x.Name,
                        ParentCategoryList = x.ParentCategoryList,
                        SiteCoreId = x.SitecoreId,
                    }).ToList();

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
                    selectOptions.Add(new Selection { DisplayName = optionDisplayName, Name = category.Name });

                    GetCategories(category.SiteCoreId, allCategories, optionDisplayName, ref selectOptions);
                }
            }
        }
    }

    internal struct Category
    {
        public string Name { get; set; }
        public string ParentCategoryList { get; set; }
        public string SiteCoreId { get; set; }
    }
}