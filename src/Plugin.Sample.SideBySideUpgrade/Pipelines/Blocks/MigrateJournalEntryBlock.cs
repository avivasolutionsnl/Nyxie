// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrateJournalEntryBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.SideBySideUpgrade
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Journaling;
    using Sitecore.Commerce.Plugin.Orders;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines the migrate JournalEntry block.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Commerce.Core.PipelineBlock{ Sitecore.Commerce.Core.CommerceEntity,
    ///         Sitecore.Commerce.Core.CommerceEntity, Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName(SideBySideUpgradeConstants.MigrateJournalEntryBlock)]
    public class MigrateJournalEntryBlock : PipelineBlock<CommerceEntity, CommerceEntity, CommercePipelineExecutionContext>
    {
        private readonly IEntityMigrationPipeline _entityMigrationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrateJournalEntryBlock" /> class.
        /// </summary>
        /// <param name="entityMigrationPipeline">The entity migration pipeline.</param>
        public MigrateJournalEntryBlock(IEntityMigrationPipeline entityMigrationPipeline)
        {
            this._entityMigrationPipeline = entityMigrationPipeline;
        }

        /// <summary>
        /// Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override async Task<CommerceEntity> Run(CommerceEntity arg, CommercePipelineExecutionContext context)
        {
            if (arg == null)
            {
                return arg;
            }

            if (!(arg is JournalEntry))
            {
                return arg;
            }

            var journalEntry = arg as JournalEntry;

            var migrateEnvironmentArgument =
                context.CommerceContext?.GetObjects<MigrateEnvironmentArgument>()?.FirstOrDefault();
            if (migrateEnvironmentArgument == null)
            {
                return null;
            }

            var entities = new List<CommerceEntity>();

            var newEnvironment = context.CommerceContext.Environment;
            context.CommerceContext.Environment = migrateEnvironmentArgument.SourceEnvironment;

            foreach (var entity in journalEntry.Entities)
            {
                var migratedEntity = await this._entityMigrationPipeline.Run(
                    new FindEntityArgument(typeof(CommerceEntity),
                            entity.Id)
                        {ShouldCreate = false},
                    context).ConfigureAwait(false);

                if (migratedEntity is Order)
                {
                    migratedEntity.Id = $"{CommerceEntity.IdPrefix<Order>()}{entity.Id}";
                }

                entities.Add(migratedEntity);
            }

            journalEntry.Entities.Clear();
            journalEntry.Entities = entities;

            context.CommerceContext.Environment = newEnvironment;
            return journalEntry;
        }
    }
}
