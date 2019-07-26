// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpgradeCommerceDataipeline.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.InPlaceUpgrade
{
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Framework.Pipelines;
   
    /// <summary>
    /// Defines the upgrade commerce data pipeline
    /// </summary>
    /// <seealso cref="Plugin.Sample.InPlaceUpgrade.IUpgradeCommerceDataPipeline" />
    public class UpgradeCommerceDataPipeline : CommercePipeline<string, bool>, IUpgradeCommerceDataPipeline
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpgradeCommerceDataPipeline" /> class.
        /// </summary>
        /// <param name="configuration">The definition.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public UpgradeCommerceDataPipeline(IPipelineConfiguration<IUpgradeCommerceDataPipeline> configuration, ILoggerFactory loggerFactory)
            : base(configuration, loggerFactory)
        {
        }
    }
}
