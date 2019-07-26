USE [SitecoreCommerce9_Global];
GO

DROP INDEX [IX_CartsLists] ON [dbo].[CartsLists];
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_CartsLists] ON [dbo].[CartsLists]
(
    [EnvironmentId] ASC,
    [ListName] ASC,
    [CommerceEntityId] ASC
) WITH (ALLOW_PAGE_LOCKS = OFF);
GO

DROP INDEX [IX_CatalogLists] ON [dbo].[CatalogLists];
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_CatalogLists] ON [dbo].[CatalogLists]
(
    [EnvironmentId] ASC,
    [ListName] ASC,
    [CommerceEntityId] ASC
) WITH (ALLOW_PAGE_LOCKS = OFF);
GO

DROP INDEX [IX_CommerceLists] ON [dbo].[CommerceLists];
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_CommerceLists] ON [dbo].[CommerceLists]
(
    [EnvironmentId] ASC,
    [ListName] ASC,
    [CommerceEntityId] ASC
) WITH (ALLOW_PAGE_LOCKS = OFF);
GO

DROP INDEX [IX_CommerceLists] ON [dbo].[ContentLists];
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ContentLists] ON [dbo].[ContentLists]
(
    [EnvironmentId] ASC,
    [ListName] ASC,
    [CommerceEntityId] ASC
) WITH (ALLOW_PAGE_LOCKS = OFF);
GO

DROP INDEX [IX_OrdersLists] ON [dbo].[OrdersLists];
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_OrdersLists] ON [dbo].[OrdersLists]
(
    [EnvironmentId] ASC,
    [ListName] ASC,
    [CommerceEntityId] ASC
) WITH (ALLOW_PAGE_LOCKS = OFF);
GO

DROP INDEX [IX_PricingLists] ON [dbo].[PricingLists];
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PricingLists] ON [dbo].[PricingLists]
(
    [EnvironmentId] ASC,
    [ListName] ASC,
    [CommerceEntityId] ASC
) WITH (ALLOW_PAGE_LOCKS = OFF);
GO

DROP INDEX [IX_PromotionsLists] ON [dbo].[PromotionsLists];
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PromotionsLists] ON [dbo].[PromotionsLists]
(
    [EnvironmentId] ASC,
    [ListName] ASC,
    [CommerceEntityId] ASC
) WITH (ALLOW_PAGE_LOCKS = OFF);
GO
