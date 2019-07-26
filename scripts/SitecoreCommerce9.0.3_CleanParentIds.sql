/******************************************************************************
* This script should run against 'SitecoreCommerce9_SharedEnvironments' to
* clean up sellable items that contain stale information about their
* parent entities.
* 
* To view entities that are affected by this, you can set the `@DryRun`
* variable to 1.
******************************************************************************/

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET NOCOUNT ON
GO

DECLARE @DryRun BIT = 0

-- Fetch all sellable items
DECLARE @Entities TABLE (
	Id VARCHAR(150),
	ParentCatalog UNIQUEIDENTIFIER,
	ParentCategory UNIQUEIDENTIFIER,
	CatalogToEntity UNIQUEIDENTIFIER,
	EnvironmentId UNIQUEIDENTIFIER,
	EntityVersion INT
)

INSERT INTO @Entities
SELECT Id, catalog.value, category.value, root.value, EnvironmentId, EntityVersion
	FROM CatalogEntities
	OUTER APPLY STRING_SPLIT(JSON_VALUE(Entity, '$.ParentCatalogList'), '|') catalog
	OUTER APPLY STRING_SPLIT(JSON_VALUE(Entity, '$.ParentCategoryList'), '|') category
	OUTER APPLY STRING_SPLIT(JSON_VALUE(Entity, '$.CatalogToEntityList'), '|') root
	WHERE Id LIKE 'Entity-SellableItem-%'

-- Fetch all catalog identifiers
DECLARE @Catalogs TABLE (
	SitecoreId UNIQUEIDENTIFIER
)

INSERT INTO @Catalogs
SELECT DISTINCT JSON_VALUE(Entity, '$.SitecoreId') FROM CatalogEntities WHERE Id LIKE 'Entity-Catalog-%'

-- Fetch all category identifiers
DECLARE @Categories TABLE (
	SitecoreId UNIQUEIDENTIFIER
)

INSERT INTO @Categories
SELECT DISTINCT JSON_VALUE(Entity, '$.SitecoreId') FROM CatalogEntities WHERE Id LIKE 'Entity-Category-%'

-- Find all entities that require modifications
DECLARE @StaleEntities TABLE (
	Id VARCHAR(150)
)

INSERT INTO @StaleEntities
SELECT Id FROM @Entities WHERE ParentCatalog NOT IN (SELECT SitecoreId FROM @Catalogs)
UNION
SELECT Id FROM @Entities WHERE CatalogToEntity NOT IN (SELECT SitecoreId FROM @Catalogs)
UNION
SELECT Id FROM @Entities WHERE ParentCategory NOT IN (SELECT SitecoreId FROM @Categories)

DELETE FROM @Entities WHERE Id NOT IN (SELECT Id FROM @StaleEntities)

-- Display entities that require an update
IF (@DryRun = 1)
BEGIN
	SELECT Id, ParentCatalog, ParentCategory, CatalogToEntity, EntityVersion FROM @Entities
END

-- Update entities
IF (@DryRun = 0)
BEGIN
	DELETE FROM @Entities WHERE ParentCatalog NOT IN (SELECT SitecoreId FROM @Catalogs)
	DELETE FROM @Entities WHERE CatalogToEntity NOT IN (SELECT SitecoreId FROM @Catalogs)
	DELETE FROM @Entities WHERE ParentCategory NOT IN (SELECT SitecoreId FROM @Categories)

	DECLARE @Id VARCHAR(150)
	DECLARE @EntityVersion INT
	DECLARE @EnvironmentId UNIQUEIDENTIFIER

	WHILE EXISTS (SELECT DISTINCT Id, EntityVersion, EnvironmentId FROM @Entities)
	BEGIN
		SELECT TOP 1 @Id = Id FROM @Entities
		SELECT TOP 1 @EntityVersion = EntityVersion FROM @Entities
		SELECT TOP 1 @EnvironmentId = EnvironmentId FROM @Entities
	
		DECLARE @ParentCatalogs NVARCHAR(MAX) = NULL
		SELECT @ParentCatalogs = COALESCE(@ParentCatalogs + '|', '') + LOWER(CONVERT(NVARCHAR(50), ParentCatalog))
		FROM (SELECT DISTINCT ParentCatalog FROM @Entities WHERE Id = @Id AND EnvironmentId = @EnvironmentId AND EntityVersion = @EntityVersion) sub

		DECLARE @ParentCategories NVARCHAR(MAX) = NULL
		SELECT @ParentCategories = COALESCE(@ParentCategories + '|', '') + LOWER(CONVERT(NVARCHAR(50), ParentCategory))
		FROM (SELECT DISTINCT ParentCategory FROM @Entities WHERE Id = @Id AND EnvironmentId = @EnvironmentId AND EntityVersion = @EntityVersion) sub

		DECLARE @CatalogEntities NVARCHAR(MAX) = NULL
		SELECT @CatalogEntities = COALESCE(@CatalogEntities + '|', '') + LOWER(CONVERT(NVARCHAR(50), CatalogToEntity))
		FROM (SELECT DISTINCT CatalogToEntity FROM @Entities WHERE Id = @Id AND EnvironmentId = @EnvironmentId AND EntityVersion = @EntityVersion) sub
	
		UPDATE CatalogEntities
		SET Entity = JSON_MODIFY(JSON_MODIFY(JSON_MODIFY(Entity, '$.ParentCatalogList', @ParentCatalogs), '$.ParentCategoryList', @ParentCategories), '$.CatalogToEntityList', @CatalogEntities)
		WHERE Id = @Id AND EnvironmentId = @EnvironmentId AND EntityVersion = @EntityVersion
		
		DELETE FROM @Entities WHERE Id = @Id AND EnvironmentId = @EnvironmentId AND EntityVersion = @EntityVersion
	END
END