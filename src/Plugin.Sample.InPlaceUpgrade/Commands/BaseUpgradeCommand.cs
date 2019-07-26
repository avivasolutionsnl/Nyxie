// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseUpgradeCommand.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>// 
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.InPlaceUpgrade
{
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Framework.Conditions;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines an upgrade commerce data command 
    /// </summary>
    /// <seealso cref="Sitecore.Commerce.Core.Commands.CommerceCommand" />
    public class BaseUpgradeCommand : CommerceCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpgradeCustomersCommand" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public BaseUpgradeCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// Processes the specified commerce context.
        /// </summary>
        /// <param name="commerceContext">The commerce context.</param>
        /// <param name="action">The action.</param>
        /// <returns>
        /// <c>true</c> if upgrade is successful; otherwise, <c>false</c>
        /// </returns>
        public override async Task ProcessWithTransaction(CommerceContext commerceContext, Func<Task> action)
        {
            var cloneContext = this.CloneCommerceContext(commerceContext);

            using (CommandActivity.Start(commerceContext, this))
            {
                await PerformTransaction(commerceContext, () => action()).ConfigureAwait(false);
            }

            MergeMessages(commerceContext, cloneContext.CommerceContext);
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
