// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomersCsConstants.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2015
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.Customers.CsMigration
{
    /// <summary>
    /// The CS Customers constants.
    /// </summary>
    public static class CustomersCsConstants
    {
        /// <summary>
        /// The get profile definition
        /// </summary>
        public const string GetProfileDefinition = "CsMigration:pipelines:getprofiledefinition";

        /// <summary>
        /// The migrate cs customer
        /// </summary>
        public const string MigrateCsCustomer = "CsMigration:pipelines:migratecscustomers";

        /// <summary>
        /// The validate customer block
        /// </summary>
        public const string ValidateCustomerBlock = "CsMigration:blocks:validatecustomer";

        /// <summary>
        /// The map addresses block
        /// </summary>
        public const string MapAddressesBlock = "CsMigration:blocks:mapaddresses";

        /// <summary>
        /// The map customer details block
        /// </summary>
        public const string MapCustomerDetailsBlock = "CsMigration:blocks:mapcustomerdetails";

        /// <summary>
        /// The get profile definition block
        /// </summary>
        public const string GetProfileDefinitionBlock = "CsMigration:blocks:getprofiledefinition";

        /// <summary>
        /// The registered plugin block
        /// </summary>
        public const string RegisteredPluginBlock = "CsMigration:blocks:registeredplugin";

        /// <summary>
        /// The configure service API block
        /// </summary>
        public const string ConfigureServiceApiBlock = "CsMigration:blocks:configureserviceapi";
    }
}
