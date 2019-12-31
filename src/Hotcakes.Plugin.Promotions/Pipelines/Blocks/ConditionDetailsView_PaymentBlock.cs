using System.Linq;
using System.Threading.Tasks;

using Hotcakes.Plugin.Promotions.Extensions;

using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Payments;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Hotcakes.Plugin.Promotions.Pipelines.Blocks
{
    public class ConditionDetailsView_PaymentBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly GetPaymentMethodsCommand _getCommand;

        public ConditionDetailsView_PaymentBlock(GetPaymentMethodsCommand getCommand)
        {
            _getCommand = getCommand;
        }

        public override async Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull(arg.Name + ": The argument cannot be null");

            var condition = arg.Properties.FirstOrDefault(p => p.Name.EqualsOrdinalIgnoreCase("Condition"));
            if (condition == null || !condition.RawValue.ToString().StartsWith("Hc_") || !condition.RawValue.ToString().EndsWith("PaymentCondition"))
            {
                return arg;
            }

            var paymentSelection = arg.Properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase("Hc_SpecificPayment"));
            if (paymentSelection != null)
            {
                var paymentMethods = await _getCommand.Process(context.CommerceContext);
                var options = paymentMethods.Select(x => new Selection { DisplayName = x.DisplayName, Name = x.Name });

                var policy = new AvailableSelectionsPolicy(options);

                paymentSelection.Policies.Add(policy);
            }

            return arg;
        }
    }
}