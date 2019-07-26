// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BraintreeClientPolicy.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.Payments.Braintree
{
    using System.Threading.Tasks;

    using Sitecore.Commerce.Core;

    /// <summary>
    /// Defines the Braintree Client Policy for Payments.
    /// </summary>
    public class BraintreeClientPolicy : Policy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BraintreeClientPolicy" /> class.
        /// </summary>
        public BraintreeClientPolicy()
        {
            this.Environment = string.Empty;
            this.MerchantId = string.Empty;
            this.PublicKey = string.Empty;
            this.PrivateKey = string.Empty;
        }

        /// <summary>
        /// Gets or sets the environment.
        /// </summary>
        /// <value>
        /// The environment.
        /// </value>
        public string Environment { get; set; }

        /// <summary>
        /// Gets or sets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        /// <value>
        /// The public key.
        /// </value>
        public string PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the private key.
        /// </summary>
        /// <value>
        /// The private key.
        /// </value>
        public string PrivateKey { get; set; }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <param name="commerceContext">The commerce context.</param>
        /// <returns>Returns true if ... is valid.</returns>
        public async Task<bool> IsValid(CommerceContext commerceContext)
        {
            if (!string.IsNullOrEmpty(this.Environment)
                && !string.IsNullOrEmpty(this.MerchantId)
                && !string.IsNullOrEmpty(this.PublicKey)
                && !string.IsNullOrEmpty(this.PrivateKey))
            {
                return true;
            }

            await commerceContext.AddMessage(
                    commerceContext.GetPolicy<KnownResultCodes>().Error,
                    "InvalidClientPolicy",
                    null,
                    "Invalid Braintree Client Policy")
                .ConfigureAwait(false);
            return false;
        }
    }
}
