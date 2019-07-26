// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpgradeCustomerBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.InPlaceUpgrade
{
    using Sitecore.Framework.Pipelines;

    using System.Threading.Tasks;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Customers;
    using Sitecore.Framework.Conditions;

    /// <summary>
    /// Defines the upgrade customer block
    /// </summary>
    ///  /// <seealso>
    /// <cref>
    ///   Sitecore.Framework.Pipelines.PipelineBlock{Sitecore.Commerce.Plugin.Customers.Customer, 
    ///   Sitecore.Commerce.Plugin.Customers.Customer, Sitecore.Commerce.Core.CommercePipelineExecutionContext}"
    /// </cref>
    /// </seealso>
    [PipelineDisplayName(InPlaceUpgradeConstants.UpgradeCustomerBlock)]
    public class UpgradeCustomerBlock : PipelineBlock<Customer, Customer, CommercePipelineExecutionContext>
    {
        private readonly IDeleteEntityPipeline _deleteEntityPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpgradeCustomerBlock"/> class.
        /// </summary>
        /// <param name="deleteEntityPipeline">The delete entity pipeline.</param>
        public UpgradeCustomerBlock(IDeleteEntityPipeline deleteEntityPipeline)
        {
            this._deleteEntityPipeline = deleteEntityPipeline;
        }

        /// <summary>
        /// Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>the Customer entity</returns>
        public override async Task<Customer> Run(Customer arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name} The customer can not be null");

            var upgradePolicy = context.CommerceContext.GetPolicy<CustomersUpgradePolicy>();

            arg.UserName = string.Concat(upgradePolicy?.CustomersDomain, "\\", arg.Email);

            await this._deleteEntityPipeline.Run(new DeleteEntityArgument($"{EntityIndex.IndexPrefix<Customer>("Id")}{arg.Email}"), context).ConfigureAwait(false);

            return arg;
        }
    }
}
