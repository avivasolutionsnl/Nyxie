// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUpgradeCommerceDataPipeline.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.InPlaceUpgrade
{
    using Sitecore.Commerce.Core;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines the Upgrade Commerce Data pipeline
    /// </summary>
    /// <seealso>
    ///   <cref>
    /// Sitecore.Framework.Pipelines.PipelineBlock{System.String,
    /// System.Boolean, Sitecore.Commerce.Core.CommercePipelineExecutionContext}"
    /// </cref>
    /// </seealso>
    [PipelineDisplayName(InPlaceUpgradeConstants.UpgradeCommerceData)]
    public interface IUpgradeCommerceDataPipeline : IPipeline<string, bool, CommercePipelineExecutionContext>
    {
    }
}
