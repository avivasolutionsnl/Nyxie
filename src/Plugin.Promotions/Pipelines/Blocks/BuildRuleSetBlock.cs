using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Hotcakes.Plugin.Promotions.Extensions;

using Microsoft.Extensions.DependencyInjection;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Rules;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Sitecore.Framework.Rules;
using Sitecore.Framework.Rules.Registry;

namespace Hotcakes.Plugin.Promotions.Pipelines.Blocks
{
    /// <summary>
    /// Copy of Sitecore.Commerce.Plugin.Rules.BuildRuleSetBlock
    /// The only changes are calling the extended functions
    /// </summary>
    [PipelineDisplayName("Rules.blocks.buildruleset")]
    public class BuildRuleSetBlock : PipelineBlock<IEnumerable<RuleModel>, RuleSet, CommercePipelineExecutionContext>
    {
        private readonly IEntityRegistry _entityRegistry;
        private IRuleBuilderInit _ruleBuilder;
        private readonly IServiceProvider _services;

        public BuildRuleSetBlock(IEntityRegistry entityRegistry, IServiceProvider services)
        {
            _entityRegistry = entityRegistry;
            _services = services;
        }

        public override async Task<RuleSet> Run(
          IEnumerable<RuleModel> arg,
          CommercePipelineExecutionContext context)
        {
            var source = arg as List<RuleModel> ?? arg.ToList();
            // ISSUE: explicit non-virtual call
            Condition.Requires(source).IsNotNull($"{Name}: The argument cannot be null");

            if (!source.Any())
            {
                var executionContext = context;
                var error = context.GetPolicy<KnownResultCodes>().Error;
                executionContext.Abort(await context.CommerceContext
                    .AddMessage(error, "RulesCannotBeNullOrEmpty", null, "Rules cannot be null or empty.")
                    .ConfigureAwait(false), context);
                return null;
            }
            _ruleBuilder = _services.GetService<IRuleBuilderInit>();
            var ruleSet1 = new RuleSet
            {
                Id = $"{CommerceEntity.IdPrefix<RuleSet>() as object}{Guid.NewGuid() as object:N}"
            };
            var ruleSet2 = ruleSet1;
            foreach (var model in source.Where(rm => rm != null))
            {
                try
                {
                    ruleSet2.Rules.Add(BuildRule(model));
                }
                catch (Exception ex)
                {
                    var executionContext = context;
                    var error = context.GetPolicy<KnownResultCodes>().Error;
                    var args = new object[] { model.Name, ex };
                    executionContext.Abort(await context.CommerceContext
                        .AddMessage(error, "RuleNotBuilt", args, $"Rule '{model.Name}' cannot be built.")
                        .ConfigureAwait(false), context);
                    return null;
                }
            }
            return ruleSet2;
        }

        protected virtual IRule BuildRule(RuleModel model)
        {
            var model1 = model.Conditions.First();
            var metadata1 = _entityRegistry.GetMetadata(model1.LibraryId);
            var ruleBuilder = _ruleBuilder.When(model1.ConvertToConditionExtended(metadata1, _entityRegistry, _services));
            for (var index = 1; index < model.Conditions.Count; ++index)
            {
                var condition1 = model.Conditions[index];
                var metadata2 = _entityRegistry.GetMetadata(condition1.LibraryId);
                var condition2 = condition1.ConvertToConditionExtended(metadata2, _entityRegistry, _services);
                if (!string.IsNullOrEmpty(condition1.ConditionOperator))
                {
                    if (condition1.ConditionOperator.ToUpperInvariant() == "OR")
                        ruleBuilder.Or(condition2);
                    else
                        ruleBuilder.And(condition2);
                }
            }
            foreach (var thenAction in model.ThenActions)
            {
                var metadata2 = _entityRegistry.GetMetadata(thenAction.LibraryId);
                var action = thenAction.ConvertToActionExtended(metadata2, _entityRegistry, _services);
                ruleBuilder.Then(action);
            }
            foreach (var elseAction in model.ElseActions)
            {
                var metadata2 = _entityRegistry.GetMetadata(elseAction.LibraryId);
                var action = elseAction.ConvertToActionExtended(metadata2, _entityRegistry, _services);
                ruleBuilder.Else(action);
            }
            return ruleBuilder.ToRule();
        }
    }
}
