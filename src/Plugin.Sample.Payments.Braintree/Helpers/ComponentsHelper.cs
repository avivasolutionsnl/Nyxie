// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComponentsHelper.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.Payments.Braintree
{
    using global::Braintree;

    using Sitecore.Commerce.Core;
    
    /// <summary>
    ///  A Components Helper to translate party for address request
    /// </summary>
    public static class ComponentsHelper
    {
        /// <summary>
        /// Translates the party to address request.
        /// </summary>
        /// <param name="party">The party.</param>
        /// <returns>A <see cref="AddressRequest"/></returns>
        internal static AddressRequest TranslatePartyToAddressRequest(Party party)
        {
            var addressRequest = new AddressRequest
            {
                CountryCodeAlpha2 = party.CountryCode,
                CountryName = party.Country,
                FirstName = party.FirstName,
                LastName = party.LastName,
                PostalCode = party.ZipPostalCode,
                StreetAddress = string.Concat(party.Address1, ",", party.Address2)
            };

            return addressRequest;
        }
    }
}
