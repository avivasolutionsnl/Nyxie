// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMigrateEnvironmentPipeline.cs" company="Sitecore Corporation">
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
    ///         Sitecore.Framework.Pipelines.IPipeline{Plugin.Sample.SideBySideUpgrade.MigrateEnvironmentArgument,      
    ///         System.Boolean,
    ///         Sitecore.Commerce.Core.CommercePipelineExecutionContext
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName(SideBySideUpgradeConstants.MigrateEnvironment)]
    public interface IMigrateEnvironmentPipeline : IPipeline<MigrateEnvironmentArgument, bool, CommercePipelineExecutionContext>
    {
    }
}
