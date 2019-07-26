// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InitializeEnvironmentPromotionsBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2018
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.AdventureWorks
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Commerce.Plugin.Coupons;
    using Sitecore.Commerce.Plugin.Fulfillment;
    using Sitecore.Commerce.Plugin.ManagedLists;
    using Sitecore.Commerce.Plugin.Promotions;
    using Sitecore.Commerce.Plugin.Rules;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines a block which bootstraps promotions for the AdventureWorks sample environment.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{System.String, System.String,
    ///         Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName(AwConstants.InitializeEnvironmentPromotionsBlock)]
    public class InitializeEnvironmentPromotionsBlock : PipelineBlock<string, string, CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistEntityPipeline;
        private readonly IAddPromotionBookPipeline _addBookPipeline;
        private readonly IAddPromotionPipeline _addPromotionPipeline;
        private readonly IAddQualificationPipeline _addQualificationPipeline;
        private readonly IAddBenefitPipeline _addBenefitPipeline;
        private readonly IAddPrivateCouponPipeline _addPrivateCouponPipeline;
        private readonly IAddPublicCouponPipeline _addPublicCouponPipeline;
        private readonly IAddPromotionItemPipeline _addPromotionItemPipeline;
        private readonly IAssociateCatalogToBookPipeline _associateCatalogToBookPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializeEnvironmentPromotionsBlock"/> class.
        /// </summary>
        /// <param name="persistEntityPipeline">The persist entity pipeline.</param>
        /// <param name="addBookPipeline">The add book pipeline.</param>
        /// <param name="addPromotionPipeline">The add promotion pipeline.</param>
        /// <param name="addQualificationPipeline">The add qualification pipeline.</param>
        /// <param name="addBenefitPipeline">The add benefit pipeline.</param>
        /// <param name="addPrivateCouponPipeline">The add private coupon pipeline.</param>
        /// <param name="addPromotionItemPipeline">The add promotion item pipeline.</param>
        /// <param name="addPublicCouponPipeline">The add public coupon pipeline.</param>
        /// <param name="associateCatalogToBookPipeline">The associate catalog to book pipeline.</param>
        public InitializeEnvironmentPromotionsBlock(
            IPersistEntityPipeline persistEntityPipeline,
            IAddPromotionBookPipeline addBookPipeline,
            IAddPromotionPipeline addPromotionPipeline,
            IAddQualificationPipeline addQualificationPipeline,
            IAddBenefitPipeline addBenefitPipeline,
            IAddPrivateCouponPipeline addPrivateCouponPipeline,
            IAddPromotionItemPipeline addPromotionItemPipeline,
            IAddPublicCouponPipeline addPublicCouponPipeline,
            IAssociateCatalogToBookPipeline associateCatalogToBookPipeline)
        {
            this._persistEntityPipeline = persistEntityPipeline;
            this._addBookPipeline = addBookPipeline;
            this._addPromotionPipeline = addPromotionPipeline;
            this._addQualificationPipeline = addQualificationPipeline;
            this._addBenefitPipeline = addBenefitPipeline;
            this._addPrivateCouponPipeline = addPrivateCouponPipeline;
            this._addPromotionItemPipeline = addPromotionItemPipeline;
            this._addPublicCouponPipeline = addPublicCouponPipeline;
            this._associateCatalogToBookPipeline = associateCatalogToBookPipeline;
        }

        /// <summary>
        /// The run.
        /// </summary>
        /// <param name="arg">
        /// The argument.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override async Task<string> Run(string arg, CommercePipelineExecutionContext context)
        {
            var artifactSet = "Environment.AdventureWorks.Promotions-1.0";

            // Check if this environment has subscribed to this Artifact Set
            if (!context.GetPolicy<EnvironmentInitializationPolicy>()
                .InitialArtifactSets.Contains(artifactSet))
            {
                return arg;
            }

            context.Logger.LogInformation($"{this.Name}.InitializingArtifactSet: ArtifactSet={artifactSet}");

            var book =
                await _addBookPipeline.Run(
                    new AddPromotionBookArgument("AdventureWorksPromotionBook")
                    {
                        DisplayName = "Adventure Works",
                        Description = "This is the Adventure Works promotion book."
                    },
                    context).ConfigureAwait(false);

            await this.CreateCartFreeShippingPromotion(book, context).ConfigureAwait(false);
            await this.CreateCartExclusive5PctOffCouponPromotion(book, context).ConfigureAwait(false);
            await this.CreateCartExclusive5OffCouponPromotion(book, context).ConfigureAwait(false);
            await this.CreateCartExclusiveGalaxyPromotion(book, context).ConfigureAwait(false);
            await this.CreateCart15PctOffCouponPromotion(book, context).ConfigureAwait(false);
            await this.CreateDisabledPromotion(book, context).ConfigureAwait(false);

            var date = DateTimeOffset.UtcNow;
            await CreateCart10PctOffCouponPromotion(book, context, date).ConfigureAwait(false);
            System.Threading.Thread.Sleep(1); //// TO ENSURE CREATING DATE IS DIFFERENT BETWEEN THESE TWO PROMOTIONS
            await CreateCart10OffCouponPromotion(book, context, date).ConfigureAwait(false);

            await this.CreateLineSaharaPromotion(book, context).ConfigureAwait(false);
            await this.CreateLineSahara5OffPromotion(book, context).ConfigureAwait(false);
            await this.CreateLineExclusiveAlpinePromotion(book, context).ConfigureAwait(false);
            await this.CreateLineExclusive20PctOffCouponPromotion(book, context).ConfigureAwait(false);
            await this.CreateLineExclusive20OffCouponPromotion(book, context).ConfigureAwait(false);
            await this.CreateLine5PctOffCouponPromotion(book, context).ConfigureAwait(false);
            await this.CreateLine5OffCouponPromotion(book, context).ConfigureAwait(false);

            await this.CreateSamplePrivateCouponPromotion(book, context).ConfigureAwait(false);
            await this.CreateLinePantsPricePromotion(book, context).ConfigureAwait(false);
            await this.AssociateCatalogToBook(book.Name, "Adventure Works Catalog", context).ConfigureAwait(false);

            return arg;
        }

        #region Cart's Promotions

        /// <summary>
        /// Creates cart free shipping promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateCartFreeShippingPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
                await _addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "CartFreeShippingPromotion", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1), "Free Shipping", "Free Shipping")
                    {
                        DisplayName = "Free Shipping",
                        Description = "Free shipping when Cart subtotal of $100 or more"
                    },
                    context).ConfigureAwait(false);

            promotion =
                await _addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartSubtotalCondition,
                            Name = CartsConstants.CartSubtotalCondition,
                            Properties = new List<PropertyModel>
                            {
                                new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                new PropertyModel { Name = "Subtotal", Value = "100", IsOperator = false, DisplayType = "System.Decimal" }
                            }
                        }),
                    context).ConfigureAwait(false);

            promotion =
                await _addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = FulfillmentConstants.CartHasFulfillmentCondition,
                            Name = FulfillmentConstants.CartHasFulfillmentCondition,
                            Properties = new List<PropertyModel>()
                        }),
                    context).ConfigureAwait(false);

            await _addBenefitPipeline.Run(
                new PromotionActionModelArgument(
                    promotion,
                    new ActionModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        LibraryId = FulfillmentConstants.CartFreeShippingAction,
                        Name = FulfillmentConstants.CartFreeShippingAction
                    }),
                context).ConfigureAwait(false);

            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates cart exclusive 5 percent off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateCartExclusive5PctOffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
                await _addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "Cart5PctOffExclusiveCouponPromotion", DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddYears(1), "5% Off Cart (Exclusive Coupon)", "5% Off Cart (Exclusive Coupon)")
                    {
                        IsExclusive = true,
                        DisplayName = "5% Off Cart (Exclusive Coupon)",
                        Description = "5% off Cart with subtotal of $10 or more (Exclusive Coupon)"
                    },
                    context).ConfigureAwait(false);

            promotion =
                await _addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartAnyItemSubtotalCondition,
                            Name = CartsConstants.CartAnyItemSubtotalCondition,
                            Properties = new List<PropertyModel>
                            {
                                new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                new PropertyModel { Name = "Subtotal", Value = "10", IsOperator = false, DisplayType = "System.Decimal" }
                            }
                        }),
                    context).ConfigureAwait(false);

            promotion =
                await _addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        action: new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartSubtotalPercentOffAction,
                            Name = CartsConstants.CartSubtotalPercentOffAction,
                            Properties = new List<PropertyModel>
                                                 {
                                                     new PropertyModel { Name = "PercentOff", Value = "5", DisplayType = "System.Decimal" }
                                                 }
                        }),
                    context).ConfigureAwait(false);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "RTRNEC5P"), context).ConfigureAwait(false);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates the cart exclusive5 off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateCartExclusive5OffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
                await _addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "Cart5OffExclusiveCouponPromotion", DateTimeOffset.UtcNow.AddDays(-3), DateTimeOffset.UtcNow.AddYears(1), "$5 Off Cart (Exclusive Coupon)", "$5 Off Cart (Exclusive Coupon)")
                    {
                        IsExclusive = true,
                        DisplayName = "$5 Off Cart (Exclusive Coupon)",
                        Description = "$5 off Cart with subtotal of $10 or more (Exclusive Coupon)"
                    },
                    context).ConfigureAwait(false);

            promotion =
                await _addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartAnyItemSubtotalCondition,
                            Name = CartsConstants.CartAnyItemSubtotalCondition,
                            Properties = new List<PropertyModel>
                                                 {
                                                     new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                                     new PropertyModel { Name = "Subtotal", Value = "10", IsOperator = false, DisplayType = "System.Decimal" }
                                                 }
                        }),
                    context).ConfigureAwait(false);

            promotion =
                await _addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartSubtotalAmountOffAction,
                            Name = CartsConstants.CartSubtotalAmountOffAction,
                            Properties = new List<PropertyModel>
                                                 {
                                                     new PropertyModel { Name = "AmountOff", Value = "5", DisplayType = "System.Decimal" }
                                                 }
                        }),
                    context).ConfigureAwait(false);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "RTRNEC5A"), context).ConfigureAwait(false);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates cart exclusive galaxy promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateCartExclusiveGalaxyPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
                await _addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "CartGalaxyTentExclusivePromotion", DateTimeOffset.UtcNow.AddDays(-4), DateTimeOffset.UtcNow.AddYears(1), "Galaxy Tent 50% Off Cart (Exclusive)", "Galaxy Tent 50% Off Cart (Exclusive)")
                    {
                        IsExclusive = true,
                        DisplayName = "Galaxy Tent 50% Off Cart (Exclusive)",
                        Description = "50% off Cart when buying Galaxy Tent (Exclusive)"
                    },
                    context).ConfigureAwait(false);

            promotion = await _addPromotionItemPipeline.Run(
                            new PromotionItemArgument(
                                promotion,
                                "Adventure Works Catalog|AW535 11|"),
                            context).ConfigureAwait(false);

            await _addBenefitPipeline.Run(
                new PromotionActionModelArgument(
                    promotion,
                    new ActionModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        LibraryId = CartsConstants.CartSubtotalPercentOffAction,
                        Name = CartsConstants.CartSubtotalPercentOffAction,
                        Properties = new List<PropertyModel>
                                             {
                                                 new PropertyModel { Name = "PercentOff", Value = "50", IsOperator = false, DisplayType = "System.Decimal" }
                                             }
                    }),
                context).ConfigureAwait(false);

            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates cart 15 percent off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateCart15PctOffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion = await this._addPromotionPipeline.Run(new AddPromotionArgument(book, "Cart15PctOffCouponPromotion", DateTimeOffset.UtcNow.AddDays(-5), DateTimeOffset.UtcNow.AddYears(1), "15% Off Cart (Coupon)", "15% Off Cart (Coupon)") { DisplayName = "15% Off Cart (Coupon)", Description = "15% off Cart with subtotal of $50 or more (Coupon)" }, context).ConfigureAwait(false);

            promotion = await this._addQualificationPipeline.Run(new PromotionConditionModelArgument(promotion, new ConditionModel { ConditionOperator = "And", Id = Guid.NewGuid().ToString(), LibraryId = CartsConstants.CartSubtotalCondition, Name = CartsConstants.CartSubtotalCondition, Properties = new List<PropertyModel> { new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" }, new PropertyModel { Name = "Subtotal", Value = "50", IsOperator = false, DisplayType = "System.Decimal" } } }), context).ConfigureAwait(false);

            promotion = await this._addBenefitPipeline.Run(new PromotionActionModelArgument(promotion, new ActionModel { Id = Guid.NewGuid().ToString(), LibraryId = CartsConstants.CartSubtotalPercentOffAction, Name = CartsConstants.CartSubtotalPercentOffAction, Properties = new List<PropertyModel> { new PropertyModel { Name = "PercentOff", Value = "15", DisplayType = "System.Decimal" } } }), context).ConfigureAwait(false);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "RTRNC15P"), context).ConfigureAwait(false);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a sample promotion making use of private coupons
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateSamplePrivateCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion = await _addPromotionPipeline.Run(
                new AddPromotionArgument(
                    book, "SamplePrivateCouponPromotion", DateTimeOffset.UtcNow.AddDays(-5), DateTimeOffset.UtcNow.AddYears(1), "Sample Private Coupon Promotion", "Sample Private Coupon Promotion")
                {
                    DisplayName = "Sample Private Coupon Promotion",
                    Description = "Sample Private Coupon Promotion"
                },
                context).ConfigureAwait(false);

            promotion = await this._addPrivateCouponPipeline.Run(new AddPrivateCouponArgument(promotion, "SPCP_", "_22", 15), context).ConfigureAwait(false);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).ConfigureAwait(false);

            const string Code = "SingleUseCouponCode";
            await this._persistEntityPipeline.Run(
                new PersistEntityArgument(
                    new Coupon(new List<Component>
                        {
                            new ListMembershipsComponent
                            {
                                Memberships = new List<string>
                                {
                                    CommerceEntity.ListName<Coupon>(),
                                    string.Format(System.Globalization.CultureInfo.InvariantCulture, context.GetPolicy<KnownCouponsListsPolicy>().PromotionCoupons, promotion.FriendlyId),
                                    string.Format(System.Globalization.CultureInfo.InvariantCulture, context.GetPolicy<KnownCouponsListsPolicy>().PublicCoupons, promotion.FriendlyId)
                                }
                            }
                        },
                        new List<Policy> { new LimitUsagesPolicy { LimitCount = 1 } })
                    {
                        Id = $"{CommerceEntity.IdPrefix<Coupon>()}{Code}",
                        Code = Code,
                        Name = Code,
                        DisplayName = Code,
                        DateCreated = DateTimeOffset.UtcNow,
                        DateUpdated = DateTimeOffset.UtcNow,
                        Promotion = new EntityReference { EntityTarget = promotion.Id },
                        CouponType = CouponsConstants.PublicCouponTypeName,
                        FriendlyId = Code
                    }),
                context).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates the cart10 PCT off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <param name="date">The date.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateCart10PctOffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context, DateTimeOffset date)
        {
            var promotion =
                await this._addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "Cart10PctOffCouponPromotion", date, date.AddYears(1), "10% Off Cart (Coupon)", "10% Off Cart (Coupon)")
                    {
                        DisplayName = "10% Off Cart (Coupon)",
                        Description = "10% off Cart with subtotal of $50 or more (Coupon)"
                    },
                    context).ConfigureAwait(false);

            promotion =
                await this._addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartSubtotalCondition,
                            Name = CartsConstants.CartSubtotalCondition,
                            Properties = new List<PropertyModel>
                            {
                                new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                new PropertyModel { Name = "Subtotal", Value = "50", IsOperator = false, DisplayType = "System.Decimal" }
                            }
                        }),
                    context).ConfigureAwait(false);

            promotion =
                await _addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartSubtotalPercentOffAction,
                            Name = CartsConstants.CartSubtotalPercentOffAction,
                            Properties = new List<PropertyModel>
                            {
                                new PropertyModel { Name = "PercentOff", Value = "10", DisplayType = "System.Decimal" }
                            }
                        }),
                    context).ConfigureAwait(false);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "RTRNC10P"), context).ConfigureAwait(false);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates the cart10 off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <param name="date">The date.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateCart10OffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context, DateTimeOffset date)
        {
            var promotion =
                await _addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "Cart10OffCouponPromotion", date, date.AddYears(1), "$10 Off Cart (Coupon)", "$10 Off Cart (Coupon)")
                    {
                        DisplayName = "$10 Off Cart (Coupon)",
                        Description = "$10 off Cart with subtotal of $50 or more (Coupon)"
                    },
                    context).ConfigureAwait(false);

            promotion =
                await _addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartSubtotalCondition,
                            Name = CartsConstants.CartSubtotalCondition,
                            Properties = new List<PropertyModel>
                            {
                                new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                new PropertyModel { Name = "Subtotal", Value = "50", IsOperator = false, DisplayType = "System.Decimal" }
                            }
                        }),
                    context).ConfigureAwait(false);

            promotion =
                await _addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartSubtotalAmountOffAction,
                            Name = CartsConstants.CartSubtotalAmountOffAction,
                            Properties = new List<PropertyModel>
                            {
                                new PropertyModel { Name = "AmountOff", Value = "10", DisplayType = "System.Decimal" }
                            }
                        }),
                    context).ConfigureAwait(false);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "RTRNC10A"), context).ConfigureAwait(false);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates the disabled promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateDisabledPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
                await _addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "DisabledPromotion", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1), "Disabled", "Disabled")
                    {
                        DisplayName = "Disabled",
                        Description = "Disabled"
                    },
                    context).ConfigureAwait(false);

            promotion =
                await _addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartSubtotalCondition,
                            Name = CartsConstants.CartSubtotalCondition,
                            Properties = new List<PropertyModel>
                            {
                                new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                new PropertyModel { Name = "Subtotal", Value = "5", IsOperator = false, DisplayType = "System.Decimal" }
                            }
                        }),
                    context).ConfigureAwait(false);

            promotion =
                await _addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartSubtotalPercentOffAction,
                            Name = CartsConstants.CartSubtotalPercentOffAction,
                            Properties = new List<PropertyModel>
                            {
                                new PropertyModel { Name = "PercentOff", Value = "100", DisplayType = "System.Decimal" }
                            }
                        }),
                    context).ConfigureAwait(false);

            promotion.SetPolicy(new DisabledPolicy());
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).ConfigureAwait(false);
        }

        private async Task AssociateCatalogToBook(string bookName, string catalogName, CommercePipelineExecutionContext context)
        {
            var arg = new CatalogAndBookArgument(bookName, catalogName);
            await _associateCatalogToBookPipeline.Run(arg, context).ConfigureAwait(false);
        }
        #endregion

        #region Line's Promotions

        /// <summary>
        /// Creates line sahara promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateLineSaharaPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
                await _addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "LineSaharaJacketPromotion", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1), "Sahara Jacket 50% Off Item", "Sahara Jacket 50% Off Item")
                    {
                        DisplayName = "Sahara Jacket 50% Off Item",
                        Description = "50% off the Sahara Jacket item"
                    },
                    context).ConfigureAwait(false);

            promotion = await _addPromotionItemPipeline.Run(
                            new PromotionItemArgument(
                                promotion,
                                "Adventure Works Catalog|AW114 06|"),
                            context).ConfigureAwait(false);

            await _addBenefitPipeline.Run(
                new PromotionActionModelArgument(
                    promotion,
                    new ActionModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        LibraryId = CartsConstants.CartItemSubtotalPercentOffAction,
                        Name = CartsConstants.CartItemSubtotalPercentOffAction,
                        Properties = new List<PropertyModel>
                                             {
                                                 new PropertyModel { Name = "PercentOff", Value = "50", IsOperator = false, DisplayType = "System.Decimal" },
                                                 new PropertyModel { Name = "TargetItemId", Value = "Adventure Works Catalog|AW114 06|", IsOperator = false, DisplayType = "System.String" }
                                             }
                    }),
                context).ConfigureAwait(false);

            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates the line pants price promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        private async Task CreateLinePantsPricePromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
                await _addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "LinePantsPricePromotion", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1), "Pay only 5$", "Pay only 5$")
                    {
                        DisplayName = "Pay only 5$",
                        Description = "Pay only 5$"
                    },
                    context).ConfigureAwait(false);

            promotion = await _addPromotionItemPipeline.Run(
                            new PromotionItemArgument(
                                promotion,
                                "Adventure Works Catalog|AW055 01|"),
                            context).ConfigureAwait(false);

            await _addBenefitPipeline.Run(
                new PromotionActionModelArgument(
                    promotion,
                    new ActionModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        LibraryId = CartsConstants.CartItemSellPriceAction,
                        Name = CartsConstants.CartItemSellPriceAction,
                        Properties = new List<PropertyModel>
                                             {
                                                 new PropertyModel { Name = "SellPrice", Value = "5", IsOperator = false, DisplayType = "System.Decimal" },
                                                 new PropertyModel { Name = "TargetItemId", Value = "Adventure Works Catalog|AW055 01|", IsOperator = false, DisplayType = "System.String" }
                                             }
                    }),
                context).ConfigureAwait(false);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "AWSELLPRICE"), context).ConfigureAwait(false);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates the line sahara5 off promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateLineSahara5OffPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
                await _addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "LineSaharaJacket5OffPromotion", DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddYears(1), "Sahara Jacket $5 Off Item", "Sahara Jacket $5 Off Item")
                    {
                        DisplayName = "Sahara Jacket $5 Off Item",
                        Description = "$5 off the Sahara Jacket item"
                    },
                    context).ConfigureAwait(false);

            promotion = await this._addPromotionItemPipeline.Run(new PromotionItemArgument(promotion, "Adventure Works Catalog|AW114 06|"), context).ConfigureAwait(false);

            await _addBenefitPipeline.Run(
                new PromotionActionModelArgument(
                    promotion,
                    new ActionModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        LibraryId = CartsConstants.CartItemSubtotalAmountOffAction,
                        Name = CartsConstants.CartItemSubtotalAmountOffAction,
                        Properties = new List<PropertyModel>
                                             {
                                                 new PropertyModel { Name = "AmountOff", Value = "5", IsOperator = false, DisplayType = "System.Decimal" },
                                                 new PropertyModel { Name = "TargetItemId", Value = "Adventure Works Catalog|AW114 06|", IsOperator = false, DisplayType = "System.String" }
                                             }
                    }),
                context).ConfigureAwait(false);

            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates the line alpine parka exclusive promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateLineExclusiveAlpinePromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
                await _addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "LineAlpineParkaExclusivePromotion", DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddYears(1), "Alpine Parka 50% Off Item (Exclusive)", "Alpine Parka 50% Off Item (Exclusive)")
                    {
                        DisplayName = "Alpine Parka 50% Off Item (Exclusive)",
                        Description = "50% off the Alpine Parka item (Exclusive)"
                    },
                    context).ConfigureAwait(false);

            promotion = await _addPromotionItemPipeline.Run(
                            new PromotionItemArgument(
                                promotion,
                                "Adventure Works Catalog|AW188 06|"),
                            context).ConfigureAwait(false);

            await _addBenefitPipeline.Run(
                new PromotionActionModelArgument(
                    promotion,
                    new ActionModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        LibraryId = CartsConstants.CartItemSubtotalPercentOffAction,
                        Name = CartsConstants.CartItemSubtotalPercentOffAction,
                        Properties = new List<PropertyModel>
                                             {
                                                 new PropertyModel { Name = "PercentOff", Value = "50", IsOperator = false, DisplayType = "System.Decimal" },
                                                 new PropertyModel { Name = "TargetItemId", Value = "Adventure Works Catalog|AW188 06|", IsOperator = false, DisplayType = "System.String" }
                                             }
                    }),
                context).ConfigureAwait(false);

            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates line exclusive 20 percent off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateLineExclusive20PctOffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
                await _addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "Line20PctOffExclusiveCouponPromotion", DateTimeOffset.UtcNow.AddDays(-3), DateTimeOffset.UtcNow.AddYears(1), "20% Off Item (Exclusive Coupon)", "20% Off Item (Exclusive Coupon)")
                    {
                        IsExclusive = true,
                        DisplayName = "20% Off Item (Exclusive Coupon)",
                        Description = "20% off any item with subtotal of $50 or more (Exclusive Coupon)"
                    },
                    context).ConfigureAwait(false);

            promotion =
                await _addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartAnyItemSubtotalCondition,
                            Name = CartsConstants.CartAnyItemSubtotalCondition,
                            Properties = new List<PropertyModel>
                                                 {
                                                     new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                                     new PropertyModel { Name = "Subtotal", Value = "25", IsOperator = false, DisplayType = "System.Decimal" }
                                                 }
                        }),
                    context).ConfigureAwait(false);

            promotion =
                await _addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartAnyItemSubtotalPercentOffAction,
                            Name = CartsConstants.CartAnyItemSubtotalPercentOffAction,
                            Properties = new List<PropertyModel>
                                                 {
                                                     new PropertyModel { Name = "PercentOff", Value = "20", DisplayType = "System.Decimal" },
                                                     new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                                     new PropertyModel { Name = "Subtotal", Value = "25", IsOperator = false, DisplayType = "System.Decimal" }
                                                 }
                        }),
                    context).ConfigureAwait(false);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "RTRNEL20P"), context).ConfigureAwait(false);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates the line exclusive $20 off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateLineExclusive20OffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
                await _addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "Line20OffExclusiveCouponPromotion", DateTimeOffset.UtcNow.AddDays(-4), DateTimeOffset.UtcNow.AddYears(1), "$20 Off Item (Exclusive Coupon)", "$20 Off Item (Exclusive Coupon)")
                    {
                        IsExclusive = true,
                        DisplayName = "$20 Off Item (Exclusive Coupon)",
                        Description = "$20 off any item with subtotal of $50 or more (Exclusive Coupon)"
                    },
                    context).ConfigureAwait(false);

            promotion =
                await _addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartAnyItemSubtotalCondition,
                            Name = CartsConstants.CartAnyItemSubtotalCondition,
                            Properties = new List<PropertyModel>
                                                 {
                                                     new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                                     new PropertyModel { Name = "Subtotal", Value = "25", IsOperator = false, DisplayType = "System.Decimal" }
                                                 }
                        }),
                    context).ConfigureAwait(false);

            promotion =
                await _addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartAnyItemSubtotalAmountOffAction,
                            Name = CartsConstants.CartAnyItemSubtotalAmountOffAction,
                            Properties = new List<PropertyModel>
                                                 {
                                                     new PropertyModel { Name = "AmountOff", Value = "20", DisplayType = "System.Decimal" },
                                                     new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                                     new PropertyModel { Name = "Subtotal", Value = "25", IsOperator = false, DisplayType = "System.Decimal" }
                                                 }
                        }),
                    context).ConfigureAwait(false);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "RTRNEL20A"), context).ConfigureAwait(false);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates line 5 percent off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateLine5PctOffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
                await _addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "Line5PctOffCouponPromotion", DateTimeOffset.UtcNow.AddDays(-5), DateTimeOffset.UtcNow.AddYears(1), "5% Off Item (Coupon)", "5% Off Item (Coupon)")
                    {
                        DisplayName = "5% Off Item (Coupon)",
                        Description = "5% off any item with subtotal of 10$ or more (Coupon)"
                    },
                    context).ConfigureAwait(false);

            promotion =
                await _addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartAnyItemSubtotalCondition,
                            Name = CartsConstants.CartAnyItemSubtotalCondition,
                            Properties = new List<PropertyModel>
                                                 {
                                                     new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                                     new PropertyModel { Name = "Subtotal", Value = "10", IsOperator = false, DisplayType = "System.Decimal" }
                                                 }
                        }),
                    context).ConfigureAwait(false);

            promotion =
                await _addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartAnyItemSubtotalPercentOffAction,
                            Name = CartsConstants.CartAnyItemSubtotalPercentOffAction,
                            Properties = new List<PropertyModel>
                                                 {
                                                     new PropertyModel { Name = "PercentOff", Value = "5", DisplayType = "System.Decimal" },
                                                     new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                                     new PropertyModel { Name = "Subtotal", Value = "10", IsOperator = false, DisplayType = "System.Decimal" }
                                                 }
                        }),
                    context).ConfigureAwait(false);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "RTRNL5P"), context).ConfigureAwait(false);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates line 5 amount off coupon promotion.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CreateLine5OffCouponPromotion(PromotionBook book, CommercePipelineExecutionContext context)
        {
            var promotion =
                await _addPromotionPipeline.Run(
                    new AddPromotionArgument(book, "Line5OffCouponPromotion", DateTimeOffset.UtcNow.AddDays(-6), DateTimeOffset.UtcNow.AddYears(1), "$5 Off Item (Coupon)", "$5 Off Item (Coupon)")
                    {
                        DisplayName = "$5 Off Item (Coupon)",
                        Description = "$5 off any item with subtotal of $10 or more (Coupon)"
                    },
                    context).ConfigureAwait(false);

            promotion =
                await _addQualificationPipeline.Run(
                    new PromotionConditionModelArgument(
                        promotion,
                        new ConditionModel
                        {
                            ConditionOperator = "And",
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartAnyItemSubtotalCondition,
                            Name = CartsConstants.CartAnyItemSubtotalCondition,
                            Properties = new List<PropertyModel>
                                                 {
                                                     new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                                     new PropertyModel { Name = "Subtotal", Value = "10", IsOperator = false, DisplayType = "System.Decimal" }
                                                 }
                        }),
                    context).ConfigureAwait(false);

            promotion =
                await _addBenefitPipeline.Run(
                    new PromotionActionModelArgument(
                        promotion,
                        new ActionModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            LibraryId = CartsConstants.CartAnyItemSubtotalAmountOffAction,
                            Name = CartsConstants.CartAnyItemSubtotalAmountOffAction,
                            Properties = new List<PropertyModel>
                                                 {
                                                     new PropertyModel { Name = "AmountOff", Value = "5", DisplayType = "System.Decimal" },
                                                     new PropertyModel { IsOperator = true, Name = "Operator", Value = "Sitecore.Framework.Rules.DecimalGreaterThanEqualToOperator", DisplayType = "Sitecore.Framework.Rules.IBinaryOperator`2[[System.Decimal],[System.Decimal]], Sitecore.Framework.Rules.Abstractions" },
                                                     new PropertyModel { Name = "Subtotal", Value = "10", IsOperator = false, DisplayType = "System.Decimal" }
                                                 }
                        }),
                    context).ConfigureAwait(false);

            promotion = await this._addPublicCouponPipeline.Run(new AddPublicCouponArgument(promotion, "RTRNL5A"), context).ConfigureAwait(false);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await this._persistEntityPipeline.Run(new PersistEntityArgument(promotion), context).ConfigureAwait(false);
        }

        #endregion
    }
}
