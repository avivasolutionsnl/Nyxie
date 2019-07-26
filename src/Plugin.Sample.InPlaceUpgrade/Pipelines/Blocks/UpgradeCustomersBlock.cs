// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpgradeCustomersBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;

namespace Plugin.Sample.InPlaceUpgrade
{
    using Sitecore.Framework.Pipelines;

    using System.Threading.Tasks;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Customers;
    using Sitecore.Framework.Conditions;

    /// <summary>
    /// Defines the upgrade customers block
    /// </summary>
    /// <seealso>
    ///   <cref>
    /// Sitecore.Framework.Pipelines.PipelineBlock{System.String,
    /// System.Boolean, Sitecore.Commerce.Core.CommercePipelineExecutionContext}"
    /// </cref>
    /// </seealso>
    [PipelineDisplayName(InPlaceUpgradeConstants.UpgradeCustomersBlock)]
    public class UpgradeCustomersBlock : PipelineBlock<string, bool, CommercePipelineExecutionContext>
    {
        private readonly UpgradeCustomersCommand _upgradeCustomersCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpgradeCustomersBlock" /> class.
        /// </summary>
        /// <param name="upgradeCustomersCommand">The upgrade customers command.</param>
        public UpgradeCustomersBlock(UpgradeCustomersCommand upgradeCustomersCommand)
        {
            this._upgradeCustomersCommand = upgradeCustomersCommand;
        }

        /// <summary>
        /// Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if customers upgrade is successful; otherwise, <c>false</c></returns>
        public override async Task<bool> Run(string arg, CommercePipelineExecutionContext context)
        {
            await this._upgradeCustomersCommand.ProcessWithTransaction(context.CommerceContext, () => this._upgradeCustomersCommand.Process(context.CommerceContext)).ConfigureAwait(false);
            var resultCodes = context.GetPolicy<KnownResultCodes>();
            return !context.CommerceContext.GetMessages().Any(p => p.Code.Equals(resultCodes.Error, StringComparison.OrdinalIgnoreCase)
                                                  && !p.Code.Equals(resultCodes.ValidationError, StringComparison.OrdinalIgnoreCase));
        }
    }
}
