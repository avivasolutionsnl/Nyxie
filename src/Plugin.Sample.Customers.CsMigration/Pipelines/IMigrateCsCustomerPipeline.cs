// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMigrateCsCustomerPipeline.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2015
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.Customers.CsMigration
{
    using System.Data;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Customers;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines the find customer pipeline interface.
    /// </summary>
    /// <seealso cref="Sitecore.Framework.Pipelines.IPipeline{DataRow, Customer, CommercePipelineExecutionContext}" />
    [PipelineDisplayName(CustomersCsConstants.MigrateCsCustomer)]
    public interface IMigrateCsCustomerPipeline : IPipeline<DataRow, Customer, CommercePipelineExecutionContext>
    {
    }
}
