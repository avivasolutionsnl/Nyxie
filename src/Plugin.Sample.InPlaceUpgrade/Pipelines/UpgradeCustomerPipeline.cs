// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpgradeCustomerPipeline.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.InPlaceUpgrade
{
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Customers;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines the upgrade a customer pipeline.
    /// </summary>
    /// <seealso>
    /// <cref>
    ///   Sitecore.Commerce.Core.CommercePipeline{Sitecore.Commerce.Plugin.Customers.Customer, 
    ///   Sitecore.Commerce.Plugin.Customers.Customer}
    /// </cref>
    /// </seealso> 
    /// <seealso cref="Plugin.Sample.InPlaceUpgrade.IUpgradeCustomerPipeline" />
    public class UpgradeCustomerPipeline : CommercePipeline<Customer, Customer>, IUpgradeCustomerPipeline
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpgradeCustomerPipeline" /> class.
        /// </summary>
        /// <param name="configuration">The definition.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public UpgradeCustomerPipeline(IPipelineConfiguration<IUpgradeCustomerPipeline> configuration, ILoggerFactory loggerFactory)
            : base(configuration, loggerFactory)
        {
        }
    }
}
