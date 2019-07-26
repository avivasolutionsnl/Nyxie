// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpgradeCatalogRelationshipsCommand.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 2018
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.SideBySideUpgrade
{
    using System;
    using System.Threading.Tasks;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Commerce.Plugin.Catalog;

    /// <summary>
    /// Defines the upgrade catalog relationships command
    /// </summary>
    /// <seealso cref="Sitecore.Commerce.Core.Commands.CommerceCommand" />
    public class UpgradeCatalogRelationshipsCommand : CommerceCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpgradeCatalogRelationshipsCommand"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <inheritdoc />
        public UpgradeCatalogRelationshipsCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// Processes the specified commerce context.
        /// </summary>
        /// <param name="commerceContext">The commerce context.</param>
        public virtual async Task<string> Process(CommerceContext commerceContext)
        {
            using (CommandActivity.Start(commerceContext, this))
            {
                return await this.Pipeline<IBootstrapRelationshipDefinitionsPipeline>().Run(string.Empty, commerceContext.PipelineContextOptions).ConfigureAwait(false);
            }
        }
    }
}
