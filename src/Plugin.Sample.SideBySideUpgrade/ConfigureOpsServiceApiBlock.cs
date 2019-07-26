// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureOpsServiceApiBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.SideBySideUpgrade
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.OData.Builder;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines a block which configures the OData model for the plugin
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{Microsoft.AspNetCore.OData.Builder.ODataConventionModelBuilder,
    ///         Microsoft.AspNetCore.OData.Builder.ODataConventionModelBuilder,
    ///         Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName(SideBySideUpgradeConstants.ConfigureOpsServiceApiBlock)]
    public class ConfigureOpsServiceApiBlock : PipelineBlock<ODataConventionModelBuilder, ODataConventionModelBuilder, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The <see cref="ODataConventionModelBuilder" />.
        /// </returns>
        public override Task<ODataConventionModelBuilder> Run(ODataConventionModelBuilder arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");

            arg.AddEntityType(typeof(MigrateEnvironmentCommand));

            var migrateEnvironment = arg.Action("MigrateEnvironment");
            migrateEnvironment.Parameter<string>("sourceName");
            migrateEnvironment.Parameter<string>("newName");
            migrateEnvironment.Parameter<string>("newArtifactStoreId");
            migrateEnvironment.ReturnsFromEntitySet<CommerceCommand>("Commands");

            var upgradeCatalogRelationships = arg.Function("UpgradeCatalogRelationships");
            upgradeCatalogRelationships.Parameter<string>("environment");
            upgradeCatalogRelationships.ReturnsFromEntitySet<CommerceCommand>("Commands");

            return Task.FromResult(arg);
        }
    }
}
