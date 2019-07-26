// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMigrateListPipeline.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2015
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.SideBySideUpgrade
{
    using Sitecore.Commerce.Core;    
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines the migrate environment pipeline interface.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.IPipeline{Plugin.Sample.SideBySideUpgrade.MigrateListArgument,      
    ///         System.Boolean,
    ///         Sitecore.Commerce.Core.CommercePipelineExecutionContext
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName(SideBySideUpgradeConstants.MigrateList)]
    public interface IMigrateListPipeline : IPipeline<MigrateListArgument, bool, CommercePipelineExecutionContext>
    {
    }
}
