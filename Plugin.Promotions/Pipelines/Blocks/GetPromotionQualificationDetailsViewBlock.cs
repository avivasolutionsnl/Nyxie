// Decompiled with JetBrains decompiler
// Type: Sitecore.Commerce.Plugin.Promotions.GetPromotionQualificationDetailsViewBlock
// Assembly: Sitecore.Commerce.Plugin.Promotions, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DB1A33C7-4019-44E9-BA67-025AB1C75053
// Assembly location: C:\temp\CommerceAuthoring_Sc9\Sitecore.Commerce.Plugin.Promotions.dll

using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Commerce.Plugin.Rules;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Sitecore.Framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Promotions.Pipelines.Blocks
{
    [PipelineDisplayName("Promotions.block.getpromotionqualificationdetailsview")]
    public class GetPromotionQualificationDetailsViewBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly GetConditionsCommand _getConditionsCommand;
        private readonly GetOperatorsCommand _getOperatorsCommand;
        private readonly GetCategoriesCommand _getCategoriesCommand;
        private List<ConditionModel> _availableConditions;

        public GetPromotionQualificationDetailsViewBlock(
          GetConditionsCommand getConditionsCommand,
          GetOperatorsCommand getOperatorsCommand,
          GetCategoriesCommand getCategoriesCommand
          )
          : base((string)null)
        {
            this._getConditionsCommand = getConditionsCommand;
            this._getOperatorsCommand = getOperatorsCommand;
            this._getCategoriesCommand = getCategoriesCommand;
        }



        public override async Task<EntityView> Run(
          EntityView arg,
          CommercePipelineExecutionContext context)
        {
            var detailsViewBlock1 = this;
            // ISSUE: explicit non-virtual call
            Condition.Requires<EntityView>(arg).IsNotNull<EntityView>(detailsViewBlock1.Name + ": The argument cannot be null");
            EntityViewArgument request = context.CommerceContext.GetObject<EntityViewArgument>();
            if (string.IsNullOrEmpty(request?.ViewName) || !request.ViewName.Equals(context.GetPolicy<KnownPromotionsViewsPolicy>().QualificationDetails, StringComparison.OrdinalIgnoreCase) && !request.ViewName.Equals(context.GetPolicy<KnownPromotionsViewsPolicy>().Master, StringComparison.OrdinalIgnoreCase) && !request.ViewName.Equals(context.GetPolicy<KnownPromotionsViewsPolicy>().Qualifications, StringComparison.OrdinalIgnoreCase))
                return arg;
            IEnumerable<ConditionModel> source1 = await detailsViewBlock1._getConditionsCommand.Process(context.CommerceContext, typeof(ICondition)).ConfigureAwait(false);
            detailsViewBlock1._availableConditions = source1 != null ? source1.ToList<ConditionModel>() : (List<ConditionModel>)null;
            bool isSelectAction = request.ForAction.Equals(context.GetPolicy<KnownPromotionsActionsPolicy>().SelectQualification, StringComparison.OrdinalIgnoreCase);
            if (isSelectAction && request.ViewName.Equals(context.GetPolicy<KnownPromotionsViewsPolicy>().QualificationDetails, StringComparison.OrdinalIgnoreCase))
            {
                await detailsViewBlock1.PopulateQualificationDetails(arg, (ConditionModel)null, true, false, context).ConfigureAwait(false);
                return arg;
            }
            bool isEditAction = request.ForAction.Equals(context.GetPolicy<KnownPromotionsActionsPolicy>().EditQualification, StringComparison.OrdinalIgnoreCase);
            if (!(request.Entity is Promotion) || !isEditAction && !string.IsNullOrEmpty(request.ForAction) || request.ViewName.Equals(context.GetPolicy<KnownPromotionsViewsPolicy>().QualificationDetails, StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(request.ItemId))
                return arg;
            Promotion entity = (Promotion)request.Entity;
            IList<ConditionModel> qualifications = entity.HasPolicy<PromotionQualificationsPolicy>() ? entity.GetPolicy<PromotionQualificationsPolicy>().Qualifications : (IList<ConditionModel>)null;
            if (request.ViewName.Equals(context.GetPolicy<KnownPromotionsViewsPolicy>().Master, StringComparison.OrdinalIgnoreCase) || request.ViewName.Equals(context.GetPolicy<KnownPromotionsViewsPolicy>().Qualifications, StringComparison.OrdinalIgnoreCase))
            {
                EntityView entityView1 = request.ViewName.Equals(context.GetPolicy<KnownPromotionsViewsPolicy>().Master, StringComparison.OrdinalIgnoreCase) ? arg.ChildViews.Cast<EntityView>().FirstOrDefault<EntityView>((Func<EntityView, bool>)(ev => ev.Name.Equals(context.GetPolicy<KnownPromotionsViewsPolicy>().Qualifications, StringComparison.OrdinalIgnoreCase))) : arg;
                IEnumerable<EntityView> entityViews;
                if (entityView1 == null)
                {
                    entityViews = (IEnumerable<EntityView>)null;
                }
                else
                {
                    IEnumerable<EntityView> source2 = entityView1.ChildViews.OfType<EntityView>();
                    entityViews = source2 != null ? source2.Where<EntityView>((Func<EntityView, bool>)(cv => cv.Name.Equals(context.GetPolicy<KnownPromotionsViewsPolicy>().QualificationDetails, StringComparison.OrdinalIgnoreCase))) : (IEnumerable<EntityView>)null;
                }
                foreach (EntityView entityView2 in entityViews)
                {
                    EntityView qualificationView = entityView2;
                    var detailsViewBlock2 = detailsViewBlock1;
                    EntityView view = qualificationView;
                    IList<ConditionModel> source2 = qualifications;
                    ConditionModel qualification = source2 != null ? source2.FirstOrDefault<ConditionModel>((Func<ConditionModel, bool>)(s => s.Id.Equals(qualificationView.ItemId, StringComparison.OrdinalIgnoreCase))) : (ConditionModel)null;
                    int num1 = isSelectAction ? 1 : 0;
                    int num2 = isEditAction ? 1 : 0;
                    CommercePipelineExecutionContext context1 = context;
                    await detailsViewBlock2.PopulateQualificationDetails(view, qualification, num1 != 0, num2 != 0, context1).ConfigureAwait(false);
                }
                return arg;
            }
            ConditionModel conditionModel;
            if (string.IsNullOrEmpty(request.ItemId))
            {
                conditionModel = (ConditionModel)null;
            }
            else
            {
                IList<ConditionModel> source2 = qualifications;
                conditionModel = source2 != null ? source2.FirstOrDefault<ConditionModel>((Func<ConditionModel, bool>)(c => c.Id.Equals(request.ItemId, StringComparison.Ordinal))) : (ConditionModel)null;
            }
            ConditionModel qualification1 = conditionModel;
            await detailsViewBlock1.PopulateQualificationDetails(arg, qualification1, isSelectAction, isEditAction, context).ConfigureAwait(false);
            return arg;
        }

        protected virtual async Task PopulateQualificationDetails(
          EntityView view,
          ConditionModel qualification,
          bool isSelectAction,
          bool isEditAction,
          CommercePipelineExecutionContext context)
        {
            if (view == null || qualification == null && !isSelectAction)
                return;
            AvailableSelectionsPolicy availableOptions = new AvailableSelectionsPolicy(this._availableConditions == null || !this._availableConditions.Any<ConditionModel>() ? (IEnumerable<Selection>)new List<Selection>() : (isSelectAction ? (IEnumerable<Selection>)this._availableConditions.Select<ConditionModel, Selection>((Func<ConditionModel, Selection>)(c =>
            {
                return new Selection()
                {
                    DisplayName = c.Name,
                    Name = c.LibraryId
                };
            })).ToList<Selection>() : (IEnumerable<Selection>)this._availableConditions.Where<ConditionModel>((Func<ConditionModel, bool>)(s => s.LibraryId.Equals(qualification.LibraryId, StringComparison.OrdinalIgnoreCase))).Select<ConditionModel, Selection>((Func<ConditionModel, Selection>)(c =>
            {
                return new Selection()
                {
                    DisplayName = c.Name,
                    Name = c.LibraryId
                };
            })).ToList<Selection>()), false);
            ViewProperty viewProperty1 = new ViewProperty();
            viewProperty1.Name = "Condition";
            viewProperty1.IsReadOnly = !isSelectAction;
            viewProperty1.RawValue = isSelectAction ? (object)string.Empty : (object)qualification.LibraryId;
            ViewProperty conditionProperty = viewProperty1;
            if (isSelectAction)
            {
                conditionProperty.Policies = (IList<Policy>)new List<Policy>()
        {
          (Policy) availableOptions
        };
                view.Properties.Add(conditionProperty);
            }
            else
            {
                List<ViewProperty> properties1 = view.Properties;
                List<Policy> commercePolicies;
                if (!isEditAction)
                {
                    commercePolicies = new List<Policy>();
                }
                else
                {
                    commercePolicies = new List<Policy>();
                    List<Selection> selectionList = new List<Selection>();
                    Selection selection1 = new Selection();
                    selection1.DisplayName = "And";
                    selection1.Name = "And";
                    selectionList.Add(selection1);
                    Selection selection2 = new Selection();
                    selection2.DisplayName = "Or";
                    selection2.Name = "Or";
                    selectionList.Add(selection2);
                    commercePolicies.Add((Policy)new AvailableSelectionsPolicy((IEnumerable<Selection>)selectionList, false));
                }
                ViewProperty viewProperty2 = new ViewProperty(commercePolicies);
                viewProperty2.Name = "ConditionOperator";
                viewProperty2.RawValue = (object)qualification.ConditionOperator;
                viewProperty2.IsReadOnly = !isEditAction || string.IsNullOrEmpty(qualification.ConditionOperator);
                viewProperty2.IsHidden = string.IsNullOrEmpty(qualification.ConditionOperator);
                viewProperty2.IsRequired = !string.IsNullOrEmpty(qualification.ConditionOperator);
                properties1.Add(viewProperty2);
                view.Properties.Add(conditionProperty);
                foreach (PropertyModel propertyModel in qualification.Properties.Where<PropertyModel>((Func<PropertyModel, bool>)(p => p.IsOperator)))
                {
                    ViewProperty viewProperty3 = new ViewProperty();
                    viewProperty3.Name = propertyModel.Name;
                    viewProperty3.RawValue = (object)propertyModel.Value;
                    viewProperty3.IsReadOnly = !isEditAction;                   
                    viewProperty3.OriginalType = propertyModel.DisplayType;
                    ViewProperty viewProperty = viewProperty3;
                    if (isEditAction)
                    {
                        IEnumerable<OperatorModel> source1 = await this._getOperatorsCommand.Process(context.CommerceContext, propertyModel.DisplayType).ConfigureAwait(false);
                        List<OperatorModel> source2 = source1 != null ? source1.ToList<OperatorModel>() : (List<OperatorModel>)null;
                        viewProperty.GetPolicy<AvailableSelectionsPolicy>().List.Clear();
                        viewProperty.GetPolicy<AvailableSelectionsPolicy>().List.AddRange(source2 == null || !source2.Any<OperatorModel>() ? (IEnumerable<Selection>)new List<Selection>() : (IEnumerable<Selection>)source2.Select<OperatorModel, Selection>((Func<OperatorModel, Selection>)(c =>
                        {
                            return new Selection()
                            {
                                DisplayName = c.Name,
                                Name = c.Type
                            };
                        })).ToList<Selection>());
                    }
                    view.Properties.Add(viewProperty);
                    viewProperty = (ViewProperty)null;
                }
                foreach (PropertyModel propertyModel in qualification.Properties.Where<PropertyModel>((Func<PropertyModel, bool>)(p => !p.IsOperator)))
                {
                    ViewProperty viewProperty3 = new ViewProperty();
                    viewProperty3.Name = propertyModel.Name;
                    viewProperty3.RawValue = (object)propertyModel.Value;
                    viewProperty3.IsReadOnly = !isEditAction;
                    viewProperty3.OriginalType = propertyModel.DisplayType;

                    //StBo                    
                    var categories = await _getCategoriesCommand.Process(context.CommerceContext, "Habitat_Master");
                    var selections = new AvailableSelectionsPolicy();
                    selections.List.AddRange(categories.Select(c => new Selection { DisplayName = c.DisplayName, Name = c.SitecoreId }));                    
                    viewProperty3.UiType = "List";
                    viewProperty3.SetPolicy(selections);

                    view.Properties.Add(viewProperty3);
                }
                if (isEditAction)
                {
                    conditionProperty.Policies = (IList<Policy>)new List<Policy>()
          {
            (Policy) availableOptions
          };
                }
                else
                {
                    Selection selection = availableOptions.List.FirstOrDefault<Selection>();
                    if (selection != null)
                        conditionProperty.RawValue = (object)selection.DisplayName;
                    List<ViewProperty> properties2 = view.Properties;
                    ViewProperty viewProperty3 = new ViewProperty();
                    viewProperty3.Name = "ItemId";
                    viewProperty3.RawValue = (object)qualification.Id;
                    viewProperty3.IsReadOnly = true;
                    viewProperty3.IsHidden = true;
                    properties2.Add(viewProperty3);
                }
            }
        }

    }
}
