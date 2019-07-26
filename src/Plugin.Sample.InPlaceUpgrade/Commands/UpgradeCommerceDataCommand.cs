// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpgradeCommerceDataCommand.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>// 
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.InPlaceUpgrade
{
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines Upgrade Commerce Data command
    /// </summary>
    /// <seealso cref="Plugin.Sample.InPlaceUpgrade.BaseUpgradeCommand" />
    public class UpgradeCommerceDataCommand : CommerceCommand
    {
        private readonly IUpgradeCommerceDataPipeline _upgradeCommerceDataPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpgradeCommerceDataCommand" /> class.
        /// </summary>
        /// <param name="upgradeCommerceDataPipeline">The upgrade commerce data pipeline.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public UpgradeCommerceDataCommand(
            IUpgradeCommerceDataPipeline upgradeCommerceDataPipeline,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this._upgradeCommerceDataPipeline = upgradeCommerceDataPipeline;
        }

        /// <summary>
        /// Processes the specified commerce context.
        /// </summary>
        /// <param name="commerceContext">The commerce context.</param>
        /// <returns>
        /// Number of processed customers
        /// </returns>
        public virtual async Task<bool> Process(CommerceContext commerceContext)
        {
            var contextOptions = commerceContext.PipelineContextOptions;
            return await this._upgradeCommerceDataPipeline.Run(string.Empty, contextOptions).ConfigureAwait(false);
        }
    }
}
