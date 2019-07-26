// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomersUpgradePolicy.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.InPlaceUpgrade
{
    using Sitecore.Commerce.Core;

    /// <summary>
    /// Customers upgrade policy for the customers domain
    /// </summary>
    /// <seealso cref="Sitecore.Commerce.Core.Policy" />
    public class CustomersUpgradePolicy : Policy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomersUpgradePolicy"/> class.
        /// </summary>
        public CustomersUpgradePolicy()
        {
            this.CustomersDomain = "CommerceUsers";
        }

        /// <summary>
        /// Gets or sets the customers domain.
        /// </summary>
        /// <value>
        /// The customers domain.
        /// </value>
        public string CustomersDomain { get; set; }
    }
}
