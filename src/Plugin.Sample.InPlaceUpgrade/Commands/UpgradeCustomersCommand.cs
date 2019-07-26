// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpgradeCustomersCommand.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>// 
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.InPlaceUpgrade
{
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Commerce.Plugin.Customers;

    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines Upgrade Customers command
    /// </summary>
    /// <seealso cref="Plugin.Sample.InPlaceUpgrade.BaseUpgradeCommand" />
    public class UpgradeCustomersCommand : BaseUpgradeCommand
    {
        private readonly IFindEntitiesInListPipeline _findEntitiesInListPipeline;
        private readonly IUpgradeCustomerPipeline _upgradeCustomerPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpgradeCustomersCommand" /> class.
        /// </summary>
        /// <param name="findEntitiesInListPipeline">The find entities in list pipeline.</param>
        /// <param name="upgradeCustomerPipeline">The upgrade customer pipeline.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public UpgradeCustomersCommand(
            IFindEntitiesInListPipeline findEntitiesInListPipeline,
            IUpgradeCustomerPipeline upgradeCustomerPipeline,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this._findEntitiesInListPipeline = findEntitiesInListPipeline;
            this._upgradeCustomerPipeline = upgradeCustomerPipeline;
        }

        /// <summary>
        /// Processes the specified commerce context.
        /// </summary>
        /// <param name="commerceContext">The commerce context.</param>
        public async Task Process(CommerceContext commerceContext)
        {
            using (CommandActivity.Start(commerceContext, this))
            {
                var context = commerceContext.PipelineContextOptions;
                var listName = CommerceEntity.ListName<Customer>();

                var result = await this.Command<GetListMetadataCommand>().Process(commerceContext, listName).ConfigureAwait(false);
                if (result == null)
                {
                    await commerceContext.AddMessage(
                        commerceContext.GetPolicy<KnownResultCodes>().Error,
                        "EntityNotFound",
                        new object[] { listName },
                        $"There is no customers in the list {listName}.").ConfigureAwait(false);
                    return;
                }

                if (result.Count == 0)
                {
                    await context.CommerceContext.AddMessage(
                        context.CommerceContext.GetPolicy<KnownResultCodes>().Error,
                        "EntityNotFound",
                        new object[] { listName },
                        $"There is no customers in the list {listName}.").ConfigureAwait(false);
                    return;
                }

                var customersCount = 0;
                var skip = 0;
                var take = 20;

                while (customersCount < result.Count)
                {
                    customersCount += await this.UpgradeCustomersInList(context, listName, skip, take).ConfigureAwait(false);
                    skip += take;
                }
            }
        }

        /// <summary>
        /// Upgrades customers in list.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="listName">Name of the list.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns>Number of migrated customers</returns>
        protected virtual async Task<int> UpgradeCustomersInList(CommercePipelineExecutionContextOptions context, string listName, int skip, int take)
        {
            var findResult = await this._findEntitiesInListPipeline.Run(new FindEntitiesInListArgument(typeof(Customer), listName, skip, take) { LoadEntities = true }, context).ConfigureAwait(false);
            if (findResult?.List?.Items == null || !findResult.List.Items.Any())
            {
                return 0;
            }

            foreach (var item in findResult.List.Items)
            {
                var customer = (Customer) item;
                if (!string.IsNullOrEmpty(customer.UserName))
                {
                    continue;
                }
            
                var cloneContext = this.CloneCommerceContext(context.CommerceContext);
                await this._upgradeCustomerPipeline.Run(customer, cloneContext).ConfigureAwait(false);
                MergeMessages(context.CommerceContext, cloneContext.CommerceContext);
            }

            return findResult.List.Items.Count;
        }
    }
}
