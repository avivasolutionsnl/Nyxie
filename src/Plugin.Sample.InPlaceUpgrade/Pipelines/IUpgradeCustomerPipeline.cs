// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUpgradeCustomerPipeline.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.InPlaceUpgrade
{
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Customers;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines the upgrade customer pipeline interface.
    /// </summary>
    /// <seealso cref="Sitecore.Framework.Pipelines.IPipeline{Customer, Customer, CommercePipelineExecutionContext}" />
    [PipelineDisplayName(InPlaceUpgradeConstants.UpgradeCustomer)]
    public interface IUpgradeCustomerPipeline : IPipeline<Customer, Customer, CommercePipelineExecutionContext>
    {
    }
}
