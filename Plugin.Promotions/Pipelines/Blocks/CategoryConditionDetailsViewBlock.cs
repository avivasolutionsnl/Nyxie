using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Promotions.Pipelines.Blocks
{
    public class CategoryConditionDetailsViewBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        public override Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires<EntityView>(arg).IsNotNull<EntityView>(arg.Name + ": The argument cannot be null");
            EntityViewArgument entityViewArgument = context.CommerceContext.GetObject<EntityViewArgument>();
            if (string.IsNullOrEmpty(entityViewArgument?.ViewName) || !entityViewArgument.ViewName.Equals(context.GetPolicy<KnownPromotionsViewsPolicy>().QualificationDetails, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(arg);
            bool flag = entityViewArgument.ForAction.Equals(context.GetPolicy<KnownPromotionsActionsPolicy>().EditQualification, StringComparison.OrdinalIgnoreCase);
            if (!(entityViewArgument.Entity is Promotion) || !flag)
                return Task.FromResult(arg);
            Promotion entity = (Promotion)entityViewArgument.Entity;


            ViewProperty qualification = arg.Properties.FirstOrDefault<ViewProperty>((Func<ViewProperty, bool>)(p => p.Name.Equals("Qualification", StringComparison.OrdinalIgnoreCase)));
            if (qualification == null)
                return Task.FromResult(arg);
            
            if(qualification.RawValue.ToString() != "IsProductFromCategory")
            {
                return Task.FromResult(arg);
            }

            List<ViewProperty> properties = arg.Properties;
            ViewProperty viewProperty = new ViewProperty();
            viewProperty.Name = "Test";
            viewProperty.DisplayName = "Test";
            viewProperty.IsReadOnly = flag;
            viewProperty.IsRequired = true;
            properties.Add(viewProperty);

            return Task.FromResult(arg);
        }
    }
}
