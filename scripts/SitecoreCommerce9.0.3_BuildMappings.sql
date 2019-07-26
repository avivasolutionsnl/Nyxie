/******************************************************************************
* This script should run against 'SitecoreCommerce9_SharedEnvironments' to
* populate the [dbo].[Mappings] table after upgrading the schema from
* Sitecore XC 9.0.2 to 9.0.3.
******************************************************************************/

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET NOCOUNT ON
GO

-- Clean the table before rebuilding the mappings
DELETE FROM [dbo].[Mappings]
GO

DECLARE @id nvarchar(150)
DECLARE @entityVersion int
DECLARE @environmentId uniqueidentifier
DECLARE @entity nvarchar(max)

DECLARE entityCursor CURSOR
	LOCAL STATIC READ_ONLY FORWARD_ONLY
FOR
	SELECT [Id], [EntityVersion], [EnvironmentId], [Entity]
	FROM [dbo].[CatalogEntities]
	WHERE [Id] LIKE 'Entity-Catalog-%' OR [Id] LIKE 'Entity-Category-%' OR [Id] LIKE 'Entity-SellableItem-%'

OPEN entityCursor
FETCH NEXT FROM entityCursor INTO @id, @entityVersion, @environmentId, @Entity
WHILE @@FETCH_STATUS = 0
BEGIN 
	DECLARE @SitecoreId uniqueidentifier
	DECLARE @ParentCatalogList NVARCHAR(MAX)
	DECLARE @CatalogToEntityList NVARCHAR(MAX)
	DECLARE @ChildrenCategoryList NVARCHAR(MAX)
	DECLARE @ChildrenSellableItemList NVARCHAR(MAX)
	DECLARE @ParentCategoryList NVARCHAR(MAX)
	DECLARE @IsBundle bit
	DECLARE @ItemVariations NVARCHAR(MAX)

	SELECT
		@SitecoreId = json.SitecoreId,
		@ParentCatalogList = json.ParentCatalogList,
		@ParentCategoryList = json.ParentCategoryList,
		@ChildrenCategoryList = json.ChildrenCategoryList,
		@ChildrenSellableItemList = json.ChildrenSellableItemList,
		@CatalogToEntityList = json.CatalogToEntityList,
		@IsBundle = json.IsBundle,
		@ItemVariations = json.ItemVariations
	FROM OPENJSON(@Entity) WITH (
		SitecoreId uniqueidentifier'$.SitecoreId',
		ParentCatalogList NVARCHAR(MAX) '$.ParentCatalogList',
		ParentCategoryList NVARCHAR(MAX) '$.ParentCategoryList',
		ChildrenCategoryList NVARCHAR(MAX) '$.ChildrenCategoryList',
		ChildrenSellableItemList NVARCHAR(MAX) '$.ChildrenSellableItemList',
		CatalogToEntityList NVARCHAR(MAX) '$.CatalogToEntityList',
		IsBundle bit '$.IsBundle',
		ItemVariations NVARCHAR(MAX) '$.ItemVariations') AS json

	EXEC [dbo].[sp_CatalogInsertMappings] @Id, @EntityVersion, @EnvironmentId, @SitecoreId, @ParentCatalogList, @CatalogToEntityList, @ChildrenCategoryList, @ChildrenSellableItemList, @ParentCategoryList, @IsBundle, @ItemVariations
	
	FETCH NEXT FROM entityCursor INTO @id, @entityVersion, @environmentId, @Entity
END

CLOSE entityCursor;  
DEALLOCATE entityCursor; 
GO
