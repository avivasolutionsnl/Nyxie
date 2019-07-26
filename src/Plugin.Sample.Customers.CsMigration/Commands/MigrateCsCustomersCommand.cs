// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrateCsCustomersCommand.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>// 
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.Customers.CsMigration
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Commerce.Plugin.Customers;
    using Sitecore.Framework.Conditions;

    /// <summary>
    /// Defines a get user site terms command
    /// </summary>
    /// <seealso cref="Sitecore.Commerce.Core.Commands.CommerceCommand" />
    public class MigrateCsCustomersCommand : CommerceCommand
    {
        /// <summary>
        /// The _get user site terms pipeline.
        /// </summary>
        private readonly IMigrateCsCustomerPipeline _migrateCustomerPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrateCsCustomersCommand" /> class.
        /// </summary>
        /// <param name="migrateCustomerPipeline">The migrate customer pipeline.</param>
        /// <param name="serviceProvider">The service Provider.</param>
        public MigrateCsCustomersCommand(IMigrateCsCustomerPipeline migrateCustomerPipeline, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this._migrateCustomerPipeline = migrateCustomerPipeline;
        }

        /// <summary>
        /// Processes the specified commerce context.
        /// </summary>
        /// <param name="commerceContext">The commerce context.</param>
        /// <returns>User site terms</returns>
        public virtual async Task<IEnumerable<Customer>> Process(CommerceContext commerceContext)
        {
            using (CommandActivity.Start(commerceContext, this))
            {
                var sqlContext = ConnectionHelper.GetProfilesSqlContext(commerceContext);
                var rows = await sqlContext.GetAllProfiles().ConfigureAwait(false);
                var migratedCustomers = new List<Customer>();

                foreach (DataRow row in rows)
                {
                    try
                    {                       
                        var cloneContext = this.CloneCommerceContext(commerceContext);

                        var csCustomer = await _migrateCustomerPipeline.Run(row, cloneContext).ConfigureAwait(false);
                        MergeMessages(commerceContext,cloneContext.CommerceContext);
                        if (csCustomer != null)
                        {
                            migratedCustomers.Add(csCustomer);
                        }
                    }
                    catch (Exception ex)
                    {
                        await commerceContext.AddMessage(
                            commerceContext.GetPolicy<KnownResultCodes>().Error,
                            "EntityNotFound",
                            new object[] { row["u_user_id"] as string, ex },
                            $"Customer {row["u_user_id"] as string} was not migrated.").ConfigureAwait(false);
                    }
                }

                return migratedCustomers;
            }
        }

        /// <summary>
        /// Clones the commerce context.
        /// </summary>
        /// <param name="commerceContext">The commerce context.</param>
        /// <returns>Clone pipeline execution context</returns>
        protected virtual CommercePipelineExecutionContext CloneCommerceContext(CommerceContext commerceContext)
        {
            Condition.Requires(commerceContext, nameof(commerceContext)).IsNotNull();

            var commerceContextClone = new CommerceContext(commerceContext.Logger, commerceContext.TelemetryClient, commerceContext.LocalizableMessagePipeline)
            { 
                Environment = commerceContext.Environment,
                GlobalEnvironment = commerceContext.GlobalEnvironment,
                Headers = commerceContext.Headers
            };
            var commerceOptionsClone = commerceContextClone.PipelineContextOptions;
            return new CommercePipelineExecutionContext(commerceOptionsClone, commerceContext.Logger);
        }

        /// <summary>
        /// Copies messages from one context into another.
        /// </summary>
        /// <param name="targetContext">The context that will recieve the messages.</param>
        /// <param name="sourceContext">The context that is the source of the messages.</param>
        protected static void MergeMessages(CommerceContext targetContext, CommerceContext sourceContext)
        {
            Condition.Requires(targetContext, nameof(targetContext)).IsNotNull();
            if (sourceContext != null)
            {
                foreach (var message in sourceContext.GetMessages())
                {
                    targetContext.AddMessage(message);
                }
            }
        }
    }
}
