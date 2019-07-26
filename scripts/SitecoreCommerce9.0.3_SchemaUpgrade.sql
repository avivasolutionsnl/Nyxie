/******************************************************************************
* This script should run against 'SitecoreCommerce9_SharedEnvironments' and
* 'SitecoreCommerce9_Global' to upgrade from Sitecore XC 9.0.2 to 9.0.3.
******************************************************************************/

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/**************************************
* Create new data types
**************************************/


IF type_id('[dbo].[EntityIdList]') IS NULL
BEGIN
	CREATE TYPE [dbo].[EntityIdList] AS TABLE(
		[CommerceEntityId] [varchar](150) NULL,
		[EntityVersion] [int] NULL
	)
END
GO


/**************************************
* Create new tables
**************************************/

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Mappings' AND xtype='U')
BEGIN
	CREATE TABLE [dbo].[Mappings](
		[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(), 
		[EntityId] [nvarchar](150) NOT NULL,
		[EntityVersion] INT NOT NULL,
		[EnvironmentId] [uniqueidentifier] NOT NULL,
		[VariationId] [nvarchar](150) NULL,
		[SitecoreId] [uniqueidentifier] NULL,
		[DeterministicId] [uniqueidentifier] NOT NULL,
		[ParentId] [uniqueidentifier] NULL,
		[IsBundle] [bit] NULL, 
		[ParentCatalogList] NVARCHAR(MAX) NULL, 
		CONSTRAINT [PK_Mappings] PRIMARY KEY ([Id]) 
	) ON [PRIMARY]

	CREATE INDEX [Mappings]
		ON [dbo].[Mappings]
		(
			[EnvironmentId],
			[EntityId],
			[EntityVersion]
		)

	CREATE NONCLUSTERED INDEX [IX_Mappings_EntityId]
	ON [dbo].[Mappings]
	(
		[EntityId] ASC
	)
END
GO

/**************************************
* Create new procedures
**************************************/

DROP PROCEDURE IF EXISTS [dbo].[sp_CatalogGetDeterministicIdsForEntityId]
GO

CREATE PROCEDURE [dbo].[sp_CatalogGetDeterministicIdsForEntityId]
	@EntityId NVARCHAR(150)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DeterministicId
	FROM Mappings
	WITH (NOLOCK)
	WHERE EntityId = @EntityId
END
GO


DROP PROCEDURE IF EXISTS [dbo].[sp_CatalogDeleteMappings]
GO

CREATE PROCEDURE [dbo].[sp_CatalogDeleteMappings]
@Id NVARCHAR(150)
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM [dbo].[Mappings] WHERE EntityId = @Id
END
GO


DROP PROCEDURE IF EXISTS [dbo].[sp_CatalogGetMappings]
GO

CREATE PROCEDURE [dbo].[sp_CatalogGetMappings]
(
	@EnvironmentId uniqueidentifier
)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT [EntityId]
	,[EntityVersion]
	,[VariationId]
	,[Mappings].[SitecoreId]
	,[DeterministicId]
	,[ParentId]
	,[IsBundle]
	,[ParentCatalogList]
	FROM [dbo].[Mappings]
	WHERE 
	EntityId LIKE 'Entity-Catalog-%' AND ParentId IS NOT NULL AND EnvironmentId = @EnvironmentId

	UNION 

	SELECT DISTINCT [EntityId]
		,[EntityVersion]
		,[VariationId]
		,[Mappings].[SitecoreId]
		,[DeterministicId]
		,[ParentId]
		,[IsBundle]
		,[ParentCatalogList]
	FROM [dbo].[Mappings]
		CROSS APPLY
		(
			SELECT DISTINCT [SitecoreId]
			FROM [dbo].[Mappings]
			WHERE 
			EntityId LIKE 'Entity-Catalog-%' AND ParentId IS NOT NULL AND EnvironmentId = @EnvironmentId
		) as ParentCatalog
	WHERE ParentCatalogList LIKE '%' + CONVERT(NVARCHAR(150),ParentCatalog.SitecoreId) + '%'
	ORDER BY VariationId ASC
END
GO


DROP PROCEDURE IF EXISTS [dbo].[sp_CatalogGetSitecoreIdsForEntityIdList]
GO

CREATE PROCEDURE [dbo].[sp_CatalogGetSitecoreIdsForEntityIdList]
	@IdList EntityIdList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT SitecoreId
	FROM Mappings
	WHERE EntityId IN (SELECT CommerceEntityId FROM @IdList) AND VariationId IS NULL
END
GO

DROP PROCEDURE IF EXISTS [dbo].[sp_CatalogInsertMappings]
GO

CREATE PROCEDURE [dbo].[sp_CatalogInsertMappings]
	@Id NVARCHAR(150),
	@EntityVersion int,
	@EnvironmentId uniqueidentifier,
	@SitecoreId uniqueidentifier,
	@ParentCatalogList NVARCHAR(MAX),
	@CatalogToEntityList NVARCHAR(MAX),
	@ChildrenCategoryList NVARCHAR(MAX),
	@ChildrenSellableItemList NVARCHAR(MAX),
	@ParentCategoryList NVARCHAR(MAX),
	@IsBundle BIT,
	@ItemVariations NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM [dbo].[Mappings] WHERE EnvironmentId = @EnvironmentId AND EntityId = @Id AND EntityVersion = @EntityVersion

	DECLARE @CatalogMappings TABLE (
		Id NVARCHAR(150),
		EntityVersion int,
		EnvironmentId uniqueidentifier NOT NULL,
		SitecoreId uniqueidentifier,
		ParentCatalogList NVARCHAR(MAX) NULL,
		CatalogToEntityList NVARCHAR(MAX) NULL,
		ChildrenCategoryList NVARCHAR(MAX) NULL,
		ChildrenSellableItemList NVARCHAR(MAX) NULL,
		ParentCategoryList NVARCHAR(MAX) NULL,
		IsBundle bit NULL,
		ItemVariations NVARCHAR(MAX) NULL
	)

	INSERT INTO @CatalogMappings SELECT @Id, @EntityVersion, @EnvironmentId, @SitecoreId, @ParentCatalogList, @CatalogToEntityList, @ChildrenCategoryList, @ChildrenSellableItemList, @ParentCategoryList, @IsBundle, @ItemVariations

	IF(@Id LIKE 'Entity-Catalog-%')
	BEGIN
		INSERT INTO [dbo].[Mappings]
		SELECT DISTINCT
			NEWID() as Id
			,@Id as EntityId
			,@EntityVersion as EntityVersion
			,@EnvironmentId as EnvironmentId
			,NULL as VariationId
			,@SitecoreId
			,@SitecoreId as DeterministicId
			,IIF(LEN(ParentCatalog.value) > 0, ParentCatalog.value, NULL) as ParentId
			,NULL as [IsBundle]
			,@ParentCatalogList as ParentCatalogList
		FROM @CatalogMappings
			CROSS APPLY STRING_SPLIT(ParentCatalogList, '|') as ParentCatalog
	END
	ELSE IF(@Id LIKE 'Entity-Category-%')
	BEGIN
		INSERT INTO [dbo].[Mappings]
		SELECT DISTINCT
			NEWID() as Id
			,@Id as EntityId
			,@EntityVersion as EntityVersion
			,@EnvironmentId as EnvironmentId
			,NULL as VariationId
			,@SitecoreId as SitecoreId
			,@SitecoreId as DeterministicId
			,IIF(LEN(ParentCategory.value) > 0, ParentCategory.value, IIF(LEN(ParentCatalog.value) > 0, ParentCatalog.value, NULL)) as ParentId
			,NULL as [IsBundle]
			,@ParentCatalogList as ParentCatalogList
		FROM @CatalogMappings
			OUTER APPLY STRING_SPLIT(ParentCatalogList, '|') as ParentCatalog
			OUTER APPLY STRING_SPLIT(ParentCategoryList, '|') as ParentCategory
	END
	ELSE IF(@Id LIKE 'Entity-SellableItem-%')
	BEGIN
		INSERT INTO [dbo].[Mappings]
		SELECT 
			NEWID() as Id
			,@Id AS EntityId
			,@EntityVersion as EntityVersion
			,@EnvironmentId as EnvironmentId
			,NULL AS VariationId
			,@SitecoreId AS SitecoreId
			,CONVERT(UNIQUEIDENTIFIER, HASHBYTES('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', ParentCategory.value)) AS VARCHAR(100))), 2) AS DeterministicId
			,IIF(LEN(ParentCategory.value) > 0, ParentCategory.value, NULL) as ParentId
			,@IsBundle AS [IsBundle]
			,@ParentCatalogList as ParentCatalogList
		FROM @CatalogMappings
			CROSS APPLY STRING_SPLIT(ParentCategoryList, '|') AS ParentCategory
		
		UNION
		
		SELECT 
			NEWID() as Id
			,@Id AS EntityId
			,@EntityVersion as EntityVersion
			,@EnvironmentId as EnvironmentId
			,ItemVariations.value AS VariationId
			,@SitecoreId AS SitecoreId
			,CONVERT(UNIQUEIDENTIFIER, HASHBYTES('MD5', 
				CAST(CONCAT(@Id, '|', ItemVariations.value, '|', LOWER(CONVERT(UNIQUEIDENTIFIER, HASHBYTES('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', ParentCategory.value)) AS VARCHAR(100))), 2))) AS VARCHAR(200))
				)) 	 AS DeterministicId
			,CONVERT(UNIQUEIDENTIFIER, HASHBYTES('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', ParentCategory.value)) AS VARCHAR(100))), 2) AS ParentId
			,@IsBundle AS [IsBundle]
			,@ParentCatalogList as ParentCatalogList
		FROM @CatalogMappings
			OUTER APPLY STRING_SPLIT(ParentCategoryList, '|') AS ParentCategory
			CROSS APPLY STRING_SPLIT(ItemVariations, '|') AS ItemVariations
		
		UNION

		SELECT 
			NEWID() as Id
			,@Id AS EntityId
			,@EntityVersion as EntityVersion
			,@EnvironmentId as EnvironmentId
			,NULL AS VariationId
			,@SitecoreId AS SitecoreId
			,CONVERT(UNIQUEIDENTIFIER, HASHBYTES('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', ParentCatalog.value)) AS VARCHAR(100))), 2) AS DeterministicId
			,IIF(LEN(ParentCatalog.value) > 0, ParentCatalog.value, NULL) as ParentId
			,@IsBundle AS [IsBundle]
			,@ParentCatalogList as ParentCatalogList
		FROM @CatalogMappings
			CROSS APPLY STRING_SPLIT(CatalogToEntityList, '|') AS ParentCatalog
			WHERE LEN(ParentCatalog.value) > 0
		
		UNION
		
		SELECT 
			NEWID() as Id
			,@Id AS EntityId
			,@EntityVersion as EntityVersion
			,@EnvironmentId as EnvironmentId
			,ItemVariations.value AS VariationId
			,@SitecoreId AS SitecoreId
			,CONVERT(UNIQUEIDENTIFIER, HASHBYTES('MD5', 
				CAST(CONCAT(@Id, '|', ItemVariations.value, '|', LOWER(CONVERT(UNIQUEIDENTIFIER, HASHBYTES('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', ParentCatalog.value)) AS VARCHAR(100))), 2))) AS VARCHAR(200))
				)) 	 AS DeterministicId
			,CONVERT(UNIQUEIDENTIFIER, HASHBYTES('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', ParentCatalog.value)) AS VARCHAR(100))), 2) AS ParentId
			,@IsBundle AS [IsBundle]
			,@ParentCatalogList as ParentCatalogList
		FROM @CatalogMappings
			OUTER APPLY STRING_SPLIT(CatalogToEntityList, '|') AS ParentCatalog
			CROSS APPLY STRING_SPLIT(ItemVariations, '|') AS ItemVariations
			WHERE LEN(ParentCatalog.value) > 0
	END
END
GO


DROP PROCEDURE IF EXISTS [dbo].[sp_CatalogUpdateMappings]
GO

CREATE PROCEDURE [dbo].[sp_CatalogUpdateMappings]
	@Id NVARCHAR(150),
	@EntityVersion int,
	@EnvironmentId uniqueidentifier,
	@SitecoreId uniqueidentifier,
	@ParentCatalogList NVARCHAR(MAX),
	@CatalogToEntityList NVARCHAR(MAX),
	@ChildrenCategoryList NVARCHAR(MAX),
	@ChildrenSellableItemList NVARCHAR(MAX),
	@ParentCategoryList NVARCHAR(MAX),
	@IsBundle bit,
	@ItemVariations NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM [dbo].[Mappings] WHERE EnvironmentId = @EnvironmentId AND EntityId = @Id AND EntityVersion = @EntityVersion

	DECLARE @CatalogMappings TABLE (
		Id NVARCHAR(150),
		EntityVersion int,
		EnvironmentId uniqueidentifier NOT NULL,
		SitecoreId uniqueidentifier,
		ParentCatalogList NVARCHAR(MAX) NULL,
		CatalogToEntityList NVARCHAR(MAX) NULL,
		ChildrenCategoryList NVARCHAR(MAX) NULL,
		ChildrenSellableItemList NVARCHAR(MAX) NULL,
		ParentCategoryList NVARCHAR(MAX) NULL,
		IsBundle bit NULL,
		ItemVariations NVARCHAR(MAX) NULL
	)

	INSERT INTO @CatalogMappings SELECT @Id, @EntityVersion, @EnvironmentId, @SitecoreId, @ParentCatalogList, @CatalogToEntityList, @ChildrenCategoryList, @ChildrenSellableItemList, @ParentCategoryList, @IsBundle, @ItemVariations

	IF(@Id LIKE 'Entity-Catalog-%')
	BEGIN
		INSERT INTO [dbo].[Mappings]
		SELECT DISTINCT
			NEWID() as Id
			,@Id as EntityId
			,@EntityVersion as EntityVersion
			,@EnvironmentId as EnvironmentId
			,NULL as VariationId
			,@SitecoreId
			,@SitecoreId as DeterministicId
			,IIF(LEN(ParentCatalog.value) > 0, ParentCatalog.value, NULL) as ParentId
			,NULL as [IsBundle]
			,@ParentCatalogList as ParentCatalogList
		FROM @CatalogMappings
			CROSS APPLY STRING_SPLIT(ParentCatalogList, '|') as ParentCatalog
	END
	ELSE IF(@Id LIKE 'Entity-Category-%')
	BEGIN
		INSERT INTO [dbo].[Mappings]
		SELECT DISTINCT
			NEWID() as Id
			,@Id as EntityId
			,@EntityVersion as EntityVersion
			,@EnvironmentId as EnvironmentId
			,NULL as VariationId
			,@SitecoreId as SitecoreId
			,@SitecoreId as DeterministicId
			,IIF(LEN(ParentCategory.value) > 0, ParentCategory.value, IIF(LEN(ParentCatalog.value) > 0, ParentCatalog.value, NULL)) as ParentId
			,NULL as [IsBundle]
			,@ParentCatalogList as ParentCatalogList
		FROM @CatalogMappings
			OUTER APPLY STRING_SPLIT(ParentCatalogList, '|') as ParentCatalog
			OUTER APPLY STRING_SPLIT(ParentCategoryList, '|') as ParentCategory
	END
	ELSE IF(@Id LIKE 'Entity-SellableItem-%')
	BEGIN
		INSERT INTO [dbo].[Mappings]
		SELECT 
			NEWID() as Id
			,@Id as EntityId
			,@EntityVersion as EntityVersion
			,@EnvironmentId as EnvironmentId
			,NULL as VariationId
			,@SitecoreId as SitecoreId
			,CONVERT(uniqueidentifier, HashBytes('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', ParentCategory.value)) as VARCHAR(100))), 2) AS DeterministicId
			,IIF(LEN(ParentCategory.value) > 0, ParentCategory.value, NULL) as ParentId
			,@IsBundle as [IsBundle]
			,@ParentCatalogList as ParentCatalogList
		FROM @CatalogMappings
			CROSS APPLY STRING_SPLIT(ParentCategoryList, '|') as ParentCategory
		
		UNION
		
		SELECT 
			NEWID() as Id
			,@Id as EntityId
			,@EntityVersion as EntityVersion
			,@EnvironmentId as EnvironmentId
			,ItemVariations.value as VariationId
			,@SitecoreId as SitecoreId
			,CONVERT(uniqueidentifier, HashBytes('MD5', 
				CAST(CONCAT(@Id, '|', ItemVariations.value, '|', LOWER(CONVERT(uniqueidentifier, HashBytes('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', ParentCategory.value)) as VARCHAR(100))), 2))) as varchar(200))
				)) 	 AS DeterministicId
			,CONVERT(uniqueidentifier, HashBytes('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', ParentCategory.value)) as VARCHAR(100))), 2) as ParentId
			,@IsBundle as [IsBundle]
			,@ParentCatalogList as ParentCatalogList
		FROM @CatalogMappings
			OUTER APPLY STRING_SPLIT(ParentCategoryList, '|') as ParentCategory
			CROSS APPLY STRING_SPLIT(ItemVariations, '|') as ItemVariations
				
		UNION

		SELECT 
			NEWID() as Id
			,@Id as EntityId
			,@EntityVersion as EntityVersion
			,@EnvironmentId as EnvironmentId
			,NULL as VariationId
			,@SitecoreId as SitecoreId
			,CONVERT(uniqueidentifier, HashBytes('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', CatalogEntities.value)) as VARCHAR(100))), 2) AS DeterministicId
			,IIF(LEN(CatalogEntities.value) > 0, CatalogEntities.value, NULL) as ParentId
			,@IsBundle as [IsBundle]
			,@ParentCatalogList as ParentCatalogList
		FROM @CatalogMappings
			CROSS APPLY STRING_SPLIT(CatalogToEntityList, '|') as CatalogEntities
			WHERE LEN(CatalogEntities.value) > 0
		
		UNION
		
		SELECT 
			NEWID() as Id
			,@Id as EntityId
			,@EntityVersion as EntityVersion
			,@EnvironmentId as EnvironmentId
			,ItemVariations.value as VariationId
			,@SitecoreId as SitecoreId
			,CONVERT(uniqueidentifier, HashBytes('MD5', 
				CAST(CONCAT(@Id, '|', ItemVariations.value, '|', LOWER(CONVERT(uniqueidentifier, HashBytes('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', CatalogEntities.value)) as VARCHAR(100))), 2))) as varchar(200))
				)) 	 AS DeterministicId
			,CONVERT(uniqueidentifier, HashBytes('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', CatalogEntities.value)) as VARCHAR(100))), 2) as ParentId
			,@IsBundle as [IsBundle]
			,@ParentCatalogList as ParentCatalogList
		FROM @CatalogMappings
			OUTER APPLY STRING_SPLIT(CatalogToEntityList, '|') as CatalogEntities
			CROSS APPLY STRING_SPLIT(ItemVariations, '|') as ItemVariations
			WHERE LEN(CatalogEntities.value) > 0
	END
END
GO


DROP PROCEDURE IF EXISTS [dbo].[sp_CommerceDBVersionGet]
GO

CREATE PROCEDURE [dbo].[sp_CommerceDBVersionGet]
AS
BEGIN
	SET NOCOUNT ON
	SELECT TOP 1 [DBVersion] FROM [Versions]
	WITH (NOLOCK)
END
GO


DROP PROCEDURE IF EXISTS [dbo].[sp_CommerceListsClearListWithSharding]
GO

CREATE PROCEDURE [dbo].[sp_CommerceListsClearListWithSharding]
(
	@ListName NVARCHAR(150),
	@TableName NVARCHAR(150) = 'CommerceLists',
	@EnvironmentId UNIQUEIDENTIFIER	
)
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @cmd AS NVARCHAR(MAX)
	SET @cmd = N'DELETE FROM '+ QUOTENAME(OBJECT_NAME(OBJECT_ID(@TableName))) +' WHERE [EnvironmentId] = @EnvId AND [ListName] = @Name'
	EXEC sp_executesql @cmd, N'@EnvId UNIQUEIDENTIFIER, @Name NVARCHAR(150)', @EnvId = @EnvironmentId, @Name = @ListName
END
GO


DROP PROCEDURE IF EXISTS [dbo].[sp_CommerceListsSelectByEntityId]
GO

CREATE PROCEDURE [dbo].[sp_CommerceListsSelectByEntityId]
(
    @ListName nvarchar(150),
    @EnvironmentId uniqueidentifier,
    @CommerceEntityId nvarchar(150)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF(@ListName IS NULL)
    BEGIN
        SELECT [ListName], [EnvironmentId], [CommerceEntityId], [EntityVersion] FROM [CommerceLists] WITH (NOLOCK) WHERE [EnvironmentId] = @EnvironmentId AND [CommerceEntityId] = @CommerceEntityId ORDER BY [ListName] ASC, [EntityVersion] DESC;
    END;
    ELSE
    BEGIN
        SELECT [ListName], [EnvironmentId], [CommerceEntityId], [EntityVersion] FROM [CommerceLists] WITH (NOLOCK) WHERE [EnvironmentId] = @EnvironmentId AND [ListName] = @ListName AND [CommerceEntityId] = @CommerceEntityId ORDER BY [ListName] ASC, [EntityVersion] DESC;
    END;
END;
GO


DROP PROCEDURE IF EXISTS [dbo].[sp_CommerceListsSelectByEntityIdWithSharding]
GO

CREATE PROCEDURE [dbo].[sp_CommerceListsSelectByEntityIdWithSharding]
(
    @ListName nvarchar(150),
    @TableName nvarchar(150) = 'CommerceLists',
    @EnvironmentId uniqueidentifier,
    @CommerceEntityId nvarchar(150)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @command AS nvarchar(max);
    DECLARE @definitions AS nvarchar(max);

    IF(@ListName IS NULL)
    BEGIN
        SET @command = N'SELECT [ListName], [EnvironmentId], [CommerceEntityId], [EntityVersion] FROM ' + QUOTENAME(OBJECT_NAME(OBJECT_ID(@TableName))) + ' WITH (NOLOCK) WHERE [EnvironmentId] = @DynamicEnvironmentId AND [CommerceEntityId] = @DynamicCommerceEntityId ORDER BY [ListName] ASC, [EntityVersion] DESC';
    END;
    ELSE
    BEGIN
        SET @command = N'SELECT [ListName], [EnvironmentId], [CommerceEntityId], [EntityVersion] FROM ' + QUOTENAME(OBJECT_NAME(OBJECT_ID(@TableName))) + ' WITH (NOLOCK) WHERE [EnvironmentId] = @DynamicEnvironmentId AND [ListName] = @DynamicListName AND [CommerceEntityId] = @DynamicCommerceEntityId ORDER BY [ListName] ASC, [EntityVersion] DESC';
    END;

    SET @definitions = N'@DynamicEnvironmentId uniqueidentifier, @DynamicListName nvarchar(150), @DynamicCommerceEntityId nvarchar(150)';

    EXEC sp_executesql @command, @definitions, @DynamicEnvironmentId = @EnvironmentId, @DynamicListName = @ListName, @DynamicCommerceEntityId = @CommerceEntityId;
END;
GO


/**************************************
* Update modified procedures
**************************************/

DROP PROCEDURE IF EXISTS [dbo].[sp_CleanEnvironment]
GO

CREATE PROCEDURE [dbo].[sp_CleanEnvironment]
(
	@EnvironmentId uniqueidentifier
)
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [CommerceLists] WHERE EnvironmentId = @EnvironmentId
	DELETE FROM [CommerceEntities] WHERE EnvironmentId = @EnvironmentId
	DELETE FROM [Mappings] WHERE [EnvironmentId] = @EnvironmentId
END
GO


DROP PROCEDURE IF EXISTS [dbo].[sp_CleanEnvironmentWithSharding]
GO

CREATE PROCEDURE [dbo].[sp_CleanEnvironmentWithSharding]
(
	@EnvironmentId UNIQUEIDENTIFIER
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @listTableName NVARCHAR(150)

	DECLARE listTableCursor CURSOR
		LOCAL STATIC READ_ONLY FORWARD_ONLY
	FOR
	SELECT name
	FROM sys.tables
	WHERE name like '%Lists';

	DECLARE @entityTableName NVARCHAR(150)

	DECLARE entityTableCursor CURSOR
		LOCAL STATIC READ_ONLY FORWARD_ONLY
	FOR
	SELECT name
	FROM sys.tables
	WHERE name like '%Entities';

	OPEN listTableCursor
	FETCH NEXT FROM listTableCursor INTO @listTableName

	WHILE @@FETCH_STATUS = 0
	BEGIN 
		DECLARE @cmd AS NVARCHAR(max)

		SET @cmd = N'DELETE FROM '+ QUOTENAME(OBJECT_NAME(OBJECT_ID(@listTableName))) +' WHERE [EnvironmentId] = @EnvId'

		EXEC sp_executesql @cmd, N'@EnvId UNIQUEIDENTIFIER', @EnvId = @EnvironmentId
		FETCH NEXT FROM listTableCursor INTO @listTableName
	END

	OPEN entityTableCursor
	FETCH NEXT FROM entityTableCursor INTO @entityTableName

	WHILE @@FETCH_STATUS = 0
	BEGIN 
		SET @cmd =  N'DELETE FROM '+ QUOTENAME(OBJECT_NAME(OBJECT_ID(@entityTableName))) +' WHERE [EnvironmentId] = @EnvId'

		EXEC sp_executesql @cmd, N'@EnvId UNIQUEIDENTIFIER', @EnvId = @EnvironmentId
		FETCH NEXT FROM entityTableCursor INTO @entityTableName
	END

	DELETE FROM [Mappings] WHERE [EnvironmentId] = @EnvironmentId
END
GO


DROP PROCEDURE IF EXISTS [dbo].[sp_CommerceBulkDeleteAllEntitiesByListNameWithSharding];
GO

CREATE PROCEDURE [dbo].[sp_CommerceBulkDeleteAllEntitiesByListNameWithSharding]
(
    @ListName nvarchar(150),
    @TableName nvarchar(150) = 'CommerceLists',
    @EnvironmentId uniqueidentifier
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @command nvarchar(max);
    DECLARE @definitions nvarchar(max);

    /* Get all entities to delete. */

    DROP TABLE IF EXISTS ##GlobalTemporaryEntity;
    SET @command = N'SELECT [CommerceEntityId] AS [EntityId], [EnvironmentId] INTO ##GlobalTemporaryEntity FROM ' + QUOTENAME(OBJECT_NAME(OBJECT_ID(@TableName))) + ' WITH (NOLOCK) WHERE [EnvironmentId] = @DynamicEnvironmentId AND [ListName] = @DynamicListName';
    SET @definitions = N'@DynamicEnvironmentId uniqueidentifier, @DynamicListName nvarchar(150)';
    EXEC sp_executesql @command, @definitions, @DynamicEnvironmentId = @EnvironmentId, @DynamicListName = @ListName;

    /* Delete all entity references in all lists tables. */

    DECLARE listTableCursor CURSOR
        LOCAL FAST_FORWARD
        FOR SELECT [name] FROM [sys].[tables] WHERE [name] LIKE '%Lists';

    DECLARE @listTableName nvarchar(150);
    OPEN listTableCursor;
    FETCH NEXT FROM listTableCursor INTO @listTableName;

    WHILE(@@FETCH_STATUS = 0)
    BEGIN
        SET @command = N'DELETE FROM [Target] FROM ' + QUOTENAME(OBJECT_NAME(OBJECT_ID(@listTableName))) + ' AS [Target] INNER JOIN ##GlobalTemporaryEntity ON [Target].[CommerceEntityId] = ##GlobalTemporaryEntity.[EntityId]';
        EXEC sp_executesql @command;

        FETCH NEXT FROM listTableCursor INTO @listTableName;
    END;

    /* Delete all entities in all entities tables. */

    DECLARE entityTableCursor CURSOR
        LOCAL FAST_FORWARD
        FOR SELECT [name] FROM [sys].[tables] WHERE [name] LIKE '%Entities';

    DECLARE @entityTableName nvarchar(150);
    OPEN entityTableCursor;
    FETCH NEXT FROM entityTableCursor INTO @entityTableName;

    WHILE(@@FETCH_STATUS = 0)
    BEGIN
        SET @command = N'DELETE FROM [Target] FROM ' + QUOTENAME(OBJECT_NAME(OBJECT_ID(@entityTableName))) + ' AS [Target] INNER JOIN ##GlobalTemporaryEntity ON [Target].[Id] = ##GlobalTemporaryEntity.[EntityId]';
        EXEC sp_executesql @command;

        FETCH NEXT FROM entityTableCursor INTO @entityTableName;
    END;

    /* Delete specific catalog related entities in Mappings table. */

    DECLARE catalogRelatedEntityCursor CURSOR
        LOCAL FAST_FORWARD
        FOR SELECT [EntityId] FROM ##GlobalTemporaryEntity WHERE [EntityId] LIKE 'Entity-Catalog-%' OR [EntityId] LIKE 'Entity-Category-%' OR [EntityId] LIKE 'Entity-SellableItem-%';

    DECLARE @catalogRelatedEntityId nvarchar(150);
    OPEN catalogRelatedEntityCursor;
    FETCH NEXT FROM catalogRelatedEntityCursor INTO @catalogRelatedEntityId;

    WHILE(@@FETCH_STATUS = 0)
    BEGIN
        EXEC sp_CatalogDeleteMappings @catalogRelatedEntityId;

        FETCH NEXT FROM catalogRelatedEntityCursor INTO @catalogRelatedEntityId;
    END;

END;
GO


DROP PROCEDURE IF EXISTS [dbo].[sp_CommerceEntitiesDeleteWithSharding]
GO

CREATE PROCEDURE [dbo].[sp_CommerceEntitiesDeleteWithSharding]
(
	@Id NVARCHAR(150),
	@EnvironmentId UNIQUEIDENTIFIER
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @RowCount INT = 0
	DECLARE @listTableName NVARCHAR(150)

	DECLARE listTableCursor CURSOR
		LOCAL STATIC READ_ONLY FORWARD_ONLY
	FOR
	SELECT name
	FROM sys.tables
	WHERE name like '%Lists';

	DECLARE @entityTableName NVARCHAR(150)

	DECLARE entityTableCursor CURSOR
		LOCAL STATIC READ_ONLY FORWARD_ONLY
	FOR
	SELECT name
	FROM sys.tables
	WHERE name like '%Entities';

	OPEN listTableCursor
	FETCH NEXT FROM listTableCursor INTO @listTableName

	WHILE @@FETCH_STATUS = 0
	BEGIN 
		DECLARE @cmd as NVARCHAR(max)
		DECLARE @parameters AS NVARCHAR(max)

		SET @cmd = N'DELETE FROM '+ QUOTENAME(OBJECT_NAME(OBJECT_ID(@listTableName))) +' WHERE [EnvironmentId] = @EnvId AND [CommerceEntityId] =  @EntityId'
		SET @parameters = '@EnvId UNIQUEIDENTIFIER, @EntityId nvarchar(150)';

		EXEC sp_executesql @cmd, @parameters, @EnvId = @EnvironmentId, @EntityId = @Id
		SET @RowCount = @RowCount + @@ROWCOUNT
		FETCH NEXT FROM listTableCursor INTO @listTableName
	END

	OPEN entityTableCursor
	FETCH NEXT FROM entityTableCursor INTO @entityTableName

	WHILE @@FETCH_STATUS = 0
	BEGIN 
		SET @cmd = N'DELETE FROM '+ QUOTENAME(OBJECT_NAME(OBJECT_ID(@entityTableName))) +' WHERE [EnvironmentId] = @EnvId AND [Id] =  @EntityId'
		SET @parameters = '@EnvId UNIQUEIDENTIFIER, @EntityId nvarchar(150)';

		EXEC sp_executesql @cmd, @parameters, @EnvId = @EnvironmentId, @EntityId = @Id
		SET @RowCount = @RowCount + @@ROWCOUNT

		IF @entityTableName = 'CatalogEntities' AND (@Id LIKE 'Entity-Catalog-%' OR @Id LIKE 'Entity-Category-%' OR @Id LIKE 'Entity-SellableItem-%')
		BEGIN
			EXEC sp_CatalogDeleteMappings @Id
		END

		FETCH NEXT FROM entityTableCursor INTO @entityTableName
	END

	IF (@RowCount=0)
	BEGIN
		DECLARE @ErrorMsg NVARCHAR(2048) = FORMATMESSAGE('Delete error: The Entity with id (%s) does not exists.', @Id);
		THROW 50000, @ErrorMsg, 1;
	END 
END
GO


DROP PROCEDURE IF EXISTS [dbo].[sp_CommerceEntitiesInsertWithSharding]
GO

CREATE PROCEDURE [dbo].[sp_CommerceEntitiesInsertWithSharding]
(
	@Id NVARCHAR(150),
	@TableName NVARCHAR(150) = 'CommerceEntities',
	@EnvironmentId UNIQUEIDENTIFIER,
	@Version INT,
	@EntityVersion INT,
	@Entity NVARCHAR(MAX),
	@Published bit
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @cmd as NVARCHAR(MAX)

	SET @cmd = N'INSERT INTO '+ QUOTENAME(OBJECT_NAME(OBJECT_ID(@TableName))) +' ([Id],[EnvironmentId],[Version],[EntityVersion],[Entity],[Published]) VALUES (@EntityId, @EnvId, @Vers, @EntVers, @Ent, @Pub)'
			
	EXEC sp_executesql @cmd, N'@EntityId NVARCHAR(150), @EnvId UNIQUEIDENTIFIER, @Vers INT, @EntVers INT, @Ent NVARCHAR(MAX), @Pub bit', @EntityId = @Id, @EnvId = @EnvironmentId, @Vers = @Version, @EntVers = @EntityVersion, @Ent = @Entity, @Pub = @Published

	IF @TableName = 'CatalogEntities' AND (@Id LIKE 'Entity-Catalog-%' OR @Id LIKE 'Entity-Category-%' OR @Id LIKE 'Entity-SellableItem-%')
	BEGIN
		DECLARE
				@SitecoreId uniqueidentifier,
				@ParentCatalogList NVARCHAR(MAX),
				@CatalogToEntityList NVARCHAR(MAX),
				@ChildrenCategoryList NVARCHAR(MAX),
				@ChildrenSellableItemList NVARCHAR(MAX),
				@ParentCategoryList NVARCHAR(MAX),
				@IsBundle bit,
				@ItemVariations NVARCHAR(MAX)

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
	END
END
GO


DROP PROCEDURE IF EXISTS [dbo].[sp_CommerceEntitiesUpdateWithSharding]
GO

CREATE PROCEDURE [dbo].[sp_CommerceEntitiesUpdateWithSharding]
(
	@Id NVARCHAR(150),
	@TableName NVARCHAR(150) = 'CommerceEntities',
	@EnvironmentId UNIQUEIDENTIFIER,
	@Version INT,
	@EntityVersion INT,
	@Entity NVARCHAR(max),
	@Published bit
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @cmd AS NVARCHAR(max)

	SET @cmd = N'UPDATE '+ QUOTENAME(OBJECT_NAME(OBJECT_ID(@TableName))) +' SET [Entity] = @Ent, [Version] = @Vers, [Published] = @Pub WHERE [EnvironmentId] = @EnvId AND [Id] = @EntityId AND [Version] = @Vers -1 AND [EntityVersion] = @EntVers'
			
	EXEC sp_executesql @cmd, N'@EntityId NVARCHAR(150), @EnvId UNIQUEIDENTIFIER, @Vers INT, @EntVers INT, @Ent NVARCHAR(max), @Pub bit', @EntityId = @Id, @EnvId = @EnvironmentId, @Vers = @Version, @EntVers = @EntityVersion, @Ent = @Entity, @Pub = @Published

	IF (@@ROWCOUNT=0)
	BEGIN
		DECLARE @ErrorMsg NVARCHAR(2048) = FORMATMESSAGE('Concurrency error: The Entity version supplied (%i) is no longer the current version.', @Version);
		THROW 50000, @ErrorMsg, 1;
	END

	IF @TableName = 'CatalogEntities' AND (@Id LIKE 'Entity-Catalog-%' OR @Id LIKE 'Entity-Category-%' OR @Id LIKE 'Entity-SellableItem-%')
	BEGIN

		DECLARE
		@SitecoreId uniqueidentifier,
		@ParentCatalogList NVARCHAR(MAX),
		@CatalogToEntityList NVARCHAR(MAX),
		@ChildrenCategoryList NVARCHAR(MAX),
		@ChildrenSellableItemList NVARCHAR(MAX),
		@ParentCategoryList NVARCHAR(MAX),
		@IsBundle bit,
		@ItemVariations NVARCHAR(MAX)

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
			SitecoreId uniqueidentifier '$.SitecoreId',
			ParentCatalogList NVARCHAR(MAX) '$.ParentCatalogList',
			ParentCategoryList NVARCHAR(MAX) '$.ParentCategoryList',
			ChildrenCategoryList NVARCHAR(MAX) '$.ChildrenCategoryList',
			ChildrenSellableItemList NVARCHAR(MAX) '$.ChildrenSellableItemList',
			CatalogToEntityList NVARCHAR(MAX) '$.CatalogToEntityList',
			IsBundle bit '$.IsBundle',
			ItemVariations NVARCHAR(MAX) '$.ItemVariations') AS json

		EXEC [dbo].[sp_CatalogUpdateMappings] @Id, @EntityVersion, @EnvironmentId, @SitecoreId, @ParentCatalogList, @CatalogToEntityList, @ChildrenCategoryList, @ChildrenSellableItemList, @ParentCategoryList, @IsBundle, @ItemVariations;
	END
END
GO


DROP PROCEDURE IF EXISTS [dbo].[sp_CommerceListsInsert]
GO

CREATE PROCEDURE [dbo].[sp_CommerceListsInsert]
(
    @ListName nvarchar(150),
    @EnvironmentId uniqueidentifier,
    @CommerceEntityId nvarchar(150),
    @EntityVersion int = 1
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [CommerceLists]
    (
        [ListName],
        [EnvironmentId],
        [CommerceEntityId],
        [EntityVersion]
    )
    VALUES
    (
        @ListName,
        @EnvironmentId,
        @CommerceEntityId,
        @EntityVersion
    );
END;
GO


DROP PROCEDURE IF EXISTS [dbo].[sp_CommerceListsInsertWithSharding]
GO

CREATE PROCEDURE [dbo].[sp_CommerceListsInsertWithSharding]
(
    @ListName nvarchar(150),
    @TableName nvarchar(150) = 'CommerceLists',
    @EnvironmentId uniqueidentifier,
    @CommerceEntityId nvarchar(150),
    @EntityVersion int = 1
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @command AS nvarchar(max);
    DECLARE @definitions AS nvarchar(max);

    SET @command = N'INSERT INTO ' + QUOTENAME(OBJECT_NAME(OBJECT_ID(@TableName))) + ' ([ListName], [EnvironmentId], [CommerceEntityId], [EntityVersion]) VALUES (@DynamicListName, @DynamicEnvironmentId, @DynamicCommerceEntityId, @DynamicEntityVersion)';
    SET @definitions = N'@DynamicListName nvarchar(150), @DynamicEnvironmentId uniqueidentifier, @DynamicCommerceEntityId nvarchar(150), @DynamicEntityVersion int';

    EXEC sp_executesql @command, @definitions, @DynamicListName = @ListName, @DynamicEnvironmentId = @EnvironmentId, @DynamicCommerceEntityId = @CommerceEntityId, @DynamicEntityVersion = @EntityVersion;
END;
GO


DROP PROCEDURE IF EXISTS [dbo].[sp_CommerceListsSelectByRange]
GO

CREATE PROCEDURE [dbo].[sp_CommerceListsSelectByRange]
(
    @ListName nvarchar(150),
    @EnvironmentId uniqueidentifier,
    @Skip int = 0,
    @Take int = 2,
    @SortOrder int = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    IF(@SortOrder = 0)
    BEGIN
        SELECT
            [CommerceEntityId],
            MAX([EntityVersion])
        FROM
            [CommerceLists] WITH (NOLOCK)
        WHERE
            [EnvironmentId] = @EnvironmentId AND
            [ListName] = @ListName
        GROUP BY
            [CommerceEntityId]
        ORDER BY
            [CommerceEntityId] ASC
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY
        ;
    END;
    ELSE IF(@SortOrder = 1)
    BEGIN
        SELECT
            [CommerceEntityId],
            MAX([EntityVersion])
        FROM
            [CommerceLists] WITH (NOLOCK)
        WHERE
            [EnvironmentId] = @EnvironmentId AND
            [ListName] = @ListName
        GROUP BY
            [CommerceEntityId]
        ORDER BY
            [CommerceEntityId] DESC
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY
        ;
    END;
END;
GO


DROP PROCEDURE IF EXISTS [dbo].[sp_CommerceListsSelectByRangeWithSharding]
GO

CREATE PROCEDURE [dbo].[sp_CommerceListsSelectByRangeWithSharding]
(
    @ListName nvarchar(150),
    @TableName nvarchar(150) = 'CommerceLists',
    @EnvironmentId uniqueidentifier,
    @Skip int = 0,
    @Take int = 2,
    @SortOrder int = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @command AS nvarchar(max);
    DECLARE @definitions AS nvarchar(max);

    IF(@SortOrder = 0)
    BEGIN
        SET @command = N'SELECT [CommerceEntityId], MAX([EntityVersion]) FROM ' + QUOTENAME(OBJECT_NAME(OBJECT_ID(@TableName))) + ' WITH (NOLOCK) WHERE [EnvironmentId] = @DynamicEnvironmentId  AND [ListName] = @DynamicListName GROUP BY [CommerceEntityId] ORDER BY [CommerceEntityId] ASC OFFSET @DynamicSkip ROWS FETCH NEXT @DynamicTake ROWS ONLY';
    END;
    ELSE IF(@SortOrder = 1)
    BEGIN
        SET @command = N'SELECT [CommerceEntityId], MAX([EntityVersion]) FROM ' + QUOTENAME(OBJECT_NAME(OBJECT_ID(@TableName))) + ' WITH (NOLOCK) WHERE [EnvironmentId] = @DynamicEnvironmentId  AND [ListName] = @DynamicListName GROUP BY [CommerceEntityId] ORDER BY [CommerceEntityId] DESC OFFSET @DynamicSkip ROWS FETCH NEXT @DynamicTake ROWS ONLY';
    END;

    SET @definitions = N'@DynamicEnvironmentId uniqueidentifier, @DynamicListName nvarchar(150), @DynamicSkip int, @DynamicTake int';

    EXEC sp_executesql @command, @definitions, @DynamicEnvironmentId = @EnvironmentId, @DynamicListName = @ListName, @DynamicSkip = @Skip, @DynamicTake = @Take;
END;
GO



/**************************************
* Update database version
**************************************/

IF EXISTS (SELECT DBVersion FROM Versions) 
	UPDATE Versions SET DBVersion='9.0.3' 
ELSE 
	INSERT INTO Versions (DBVersion) Values('9.0.3')
GO
