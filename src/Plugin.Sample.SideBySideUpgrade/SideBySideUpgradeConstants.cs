// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SideBySideUpgradeConstants.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.SideBySideUpgrade
{
    /// <summary>
    /// The Upgradet constants.
    /// </summary>
    public static class SideBySideUpgradeConstants
    {
        /// <summary>
        /// The migrate environment
        /// </summary>
        public const string MigrateEnvironment = "SideBySideUpgrade.pipelines.migrateenvironment";

        /// <summary>
        /// The migrate environment metadata
        /// </summary>
        public const string MigrateEnvironmentMetadata = "SideBySideUpgrade.pipelines.migrateenvironmentmetadata";

        /// <summary>
        /// The migrate entity metadata
        /// </summary>
        public const string MigrateEntityMetadata = "SideBySideUpgrade.pipelines.migrateentitymetadata";

        /// <summary>
        /// The migrate list
        /// </summary>
        public const string MigrateList = "SideBySideUpgrade.pipelines.migratelist";

        /// <summary>
        /// The configure Ops service API block name.
        /// </summary>
        public const string ConfigureOpsServiceApiBlock = "SideBySideUpgrade.ConfigureOpsServiceApi";

        /// <summary>
        /// The get source environment block
        /// </summary>
        public const string GetSourceEnvironmentBlock = "SideBySideUpgrade.GetSourceEnvironment";

        /// <summary>
        /// The inject environment policies block
        /// </summary>
        public const string InjectEnvironmentPoliciesBlock = "SideBySideUpgrade.InjectEnvironmentPolicies";

        /// <summary>
        /// The migrate lists block
        /// </summary>
        public const string MigrateListsBlock = "SideBySideUpgrade.MigrateLists";

        /// <summary>
        /// The migrate list block
        /// </summary>
        public const string MigrateListBlock = "SideBySideUpgrade.MigrateList";

        /// <summary>
        /// The get source entity block
        /// </summary>
        public const string GetSourceEntityBlock = "SideBySideUpgrade.GetSourceEntity";

        /// <summary>
        /// The get target entity block
        /// </summary>
        public const string GetTargetEntityBlock = "SideBySideUpgrade.GetTargetEntityBlock";

        /// <summary>
        /// The migrate order entity block
        /// </summary>
        public const string MigrateOrderEntityBlock = "SideBySideUpgrade.MigrateOrderEntity";

        /// <summary>
        /// The migrate entity index block
        /// </summary>
        public const string MigrateEntityIndexBlock = "SideBySideUpgrade.MigrateEntityIndex";

        /// <summary>
        /// The migrate gift card block
        /// </summary>
        public const string MigrateGiftCardBlock = "SideBySideUpgrade.MigrateGiftCard";

        /// <summary>
        /// The migrate journal entry block
        /// </summary>
        public const string MigrateJournalEntryBlock = "SideBySideUpgrade.MigrateJournalEntry";

        /// <summary>
        /// The migrate sellable item block
        /// </summary>
        public const string MigrateSellableItemBlock = "SideBySideUpgrade.MigrateSellableItem";

        /// <summary>
        /// The patch environment json block
        /// </summary>
        public const string PatchEnvironmentJsonBlock = "SideBySideUpgrade.PatchEnvironmentJson";

        /// <summary>
        /// The set entity list memberships block
        /// </summary>
        public const string SetEntityListMembershipsBlock = "SideBySideUpgrade.SetEntityListMemberships";

        /// <summary>
        /// The persist migrated entity block
        /// </summary>
        public const string PersistMigratedEntityBlock = "SideBySideUpgrade.PersistMigratedEntity";

        /// <summary>
        /// The finalize environment migration block
        /// </summary>
        public const string FinalizeEnvironmentMigrationBlock = "SideBySideUpgrade.FinalizeEnvironmentMigration";
    }
}
