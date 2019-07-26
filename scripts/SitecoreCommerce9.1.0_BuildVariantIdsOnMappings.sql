/******************************************************************************
* This script should run against 'SitecoreCommerce9_SharedEnvironments' to
* populate the [dbo].[Mappings] table after upgrading the schema from
* Sitecore XC 9.0.3 to 9.1.0.
******************************************************************************/

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET NOCOUNT ON
GO

DECLARE @id nvarchar(150)
DECLARE @entityVersion int
DECLARE @published bit
DECLARE @environmentId uniqueidentifier
DECLARE @entity nvarchar(max)

DECLARE entityCursor CURSOR
	LOCAL STATIC READ_ONLY FORWARD_ONLY
FOR
	SELECT [Id], [EntityVersion], [Published], [EnvironmentId], [Entity]
	FROM [dbo].[CatalogEntities]
	WHERE [Id] LIKE 'Entity-Catalog-%' OR [Id] LIKE 'Entity-Category-%' OR [Id] LIKE 'Entity-SellableItem-%'

OPEN entityCursor
FETCH NEXT FROM entityCursor INTO @id, @entityVersion, @published, @environmentId, @Entity
WHILE @@FETCH_STATUS = 0
BEGIN 

	DECLARE @SitecoreId uniqueidentifier
	DECLARE @ParentCatalogList NVARCHAR(MAX)
	DECLARE @CatalogToEntityList NVARCHAR(MAX)
	DECLARE @ChildrenCategoryList NVARCHAR(MAX)
	DECLARE @ChildrenSellableItemList NVARCHAR(MAX)
	DECLARE @ParentCategoryList NVARCHAR(MAX)
	DECLARE @IsBundle bit

	DECLARE @ItemVariations NVARCHAR(MAX) = NULL
	DECLARE @json NVARCHAR(MAX) = JSON_QUERY(@Entity,'$.Components."$values"')

	--Get variations ids and concat with |
	SELECT @ItemVariations = COALESCE(@ItemVariations + '|', '') + VariationId
	FROM OPENJSON(@json)
	WITH (
		TypeName nvarchar(4000) '$."$type"',
		ChildComponents nvarchar(max) '$.ChildComponents."$values"' AS JSON
	) as ComponentsValues
	CROSS APPLY OPENJSON(ChildComponents)
	WITH (
		VariationId nvarchar(50) '$.Id'
	)
	WHERE
	ComponentsValues.TypeName = 'Sitecore.Commerce.Plugin.Catalog.ItemVariationsComponent, Sitecore.Commerce.Plugin.Catalog'

	--Update Entity json with variations ids
	UPDATE CatalogEntities
	SET Entity = JSON_MODIFY(@Entity, '$.ItemVariations', @ItemVariations)
	WHERE Id = @Id AND EnvironmentId = @EnvironmentId AND EntityVersion = @EntityVersion

	--Create mappings rows for each variations
	SELECT
		@SitecoreId = json.SitecoreId,
		@ParentCatalogList = json.ParentCatalogList,
		@ParentCategoryList = json.ParentCategoryList,
		@ChildrenCategoryList = json.ChildrenCategoryList,
		@ChildrenSellableItemList = json.ChildrenSellableItemList,
		@CatalogToEntityList = json.CatalogToEntityList,
		@IsBundle = json.IsBundle
	FROM OPENJSON(@Entity) WITH (
		SitecoreId uniqueidentifier'$.SitecoreId',
		ParentCatalogList NVARCHAR(MAX) '$.ParentCatalogList',
		ParentCategoryList NVARCHAR(MAX) '$.ParentCategoryList',
		ChildrenCategoryList NVARCHAR(MAX) '$.ChildrenCategoryList',
		ChildrenSellableItemList NVARCHAR(MAX) '$.ChildrenSellableItemList',
		CatalogToEntityList NVARCHAR(MAX) '$.CatalogToEntityList',
		IsBundle bit '$.IsBundle') AS json

	EXEC [dbo].[sp_CatalogInsertMappings] @Id, @EntityVersion, @Published, @EnvironmentId, @SitecoreId, @ParentCatalogList, @CatalogToEntityList, @ChildrenCategoryList, @ChildrenSellableItemList, @ParentCategoryList, @IsBundle, @ItemVariations

	FETCH NEXT FROM entityCursor INTO @id, @entityVersion, @published, @environmentId, @Entity
END

CLOSE entityCursor;  
DEALLOCATE entityCursor; 
GO
