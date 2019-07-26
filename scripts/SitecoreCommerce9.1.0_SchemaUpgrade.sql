/******************************************************************************
* This script should run against 'SitecoreCommerce9_SharedEnvironments' and
* 'SitecoreCommerce9_Global' to upgrade from Sitecore XC 9.0.3 to 9.1
******************************************************************************/

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/**************************************
* Update database version
**************************************/
UPDATE Versions SET DBVersion='9.1.0' 

/**************************************
* Update mappings table
**************************************/

IF EXISTS (SELECT * FROM sysobjects WHERE name = 'Mappings' AND xtype = 'U')
BEGIN
	DROP TABLE [dbo].[Mappings]

	CREATE TABLE [dbo].[Mappings](
		[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(), 
		[EntityId] [nvarchar](150) NOT NULL,
		[EntityVersion] INT NOT NULL,
		[Published] BIT NOT NULL DEFAULT 1,
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
        /* BEGIN: Delete entities that are dependent. */
        DECLARE entityCursor CURSOR
        LOCAL FAST_FORWARD
        FOR SELECT [EntityId] FROM ##GlobalTemporaryEntity;

        DECLARE @entityId nvarchar(150);
        OPEN entityCursor;
        FETCH NEXT FROM entityCursor INTO @entityId;

        WHILE(@@FETCH_STATUS = 0)
        BEGIN
            /* Filter for catalog related entities to delete in Mappings table. */
            IF @entityTableName = 'CatalogEntities' AND (@entityId LIKE 'Entity-Catalog-%' OR @entityId LIKE 'Entity-Category-%' OR @entityId LIKE 'Entity-SellableItem-%')
            BEGIN
                EXEC sp_CatalogDeleteMappings @entityId;
            END;

            /* Delete associated LocalizationEntity in CommerceEntities table. */
            DECLARE @entityJsonString nvarchar(max);
            SET @command = N'SELECT TOP(1) @DynamicEntityJsonString = [Entity] FROM ' + QUOTENAME(OBJECT_NAME(OBJECT_ID(@entityTableName))) + ' WITH (NOLOCK) WHERE [EnvironmentId] = @DynamicEnvironmentId AND [Id] = @DynamicId';
            SET @definitions = N'@DynamicEnvironmentId uniqueidentifier, @DynamicId nvarchar(150), @DynamicEntityJsonString nvarchar(max) OUTPUT';
            EXEC sp_executesql @command, @definitions, @DynamicEnvironmentId = @EnvironmentId, @DynamicId = @entityId, @DynamicEntityJsonString = @entityJsonString OUTPUT;
            IF(@entityJsonString IS NOT NULL)
            BEGIN
                DECLARE @localizationEntityID nvarchar(max);
                SELECT TOP(1) @localizationEntityID = JSON_VALUE([value], '$.Entity.EntityTarget') FROM OPENJSON(@entityJsonString, '$.Components."$values"') WHERE JSON_VALUE([value], '$."$type"') LIKE 'Sitecore.Commerce.Core.LocalizedEntityComponent%';
                DELETE FROM [CommerceEntities] WHERE [EnvironmentId] = @EnvironmentId AND [Id] = @localizationEntityID;
            END;

            FETCH NEXT FROM entityCursor INTO @entityId;
        END;

        CLOSE entityCursor;
        DEALLOCATE entityCursor;
        /* END: Delete entities that are dependent. */

        SET @command = N'DELETE FROM [Target] FROM ' + QUOTENAME(OBJECT_NAME(OBJECT_ID(@entityTableName))) + ' AS [Target] INNER JOIN ##GlobalTemporaryEntity ON [Target].[Id] = ##GlobalTemporaryEntity.[EntityId]';
        EXEC sp_executesql @command;

        FETCH NEXT FROM entityTableCursor INTO @entityTableName;
    END;
END;
GO

DROP PROCEDURE IF EXISTS [dbo].[sp_CatalogGetMappings];
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
	,[Published]
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
		,[Published]
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

DROP PROCEDURE IF EXISTS [dbo].[sp_CatalogInsertMappings];
GO

CREATE PROCEDURE [dbo].[sp_CatalogInsertMappings]
	@Id NVARCHAR(150),
	@EntityVersion int,
	@Published bit,
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
		Published bit,
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

	INSERT INTO @CatalogMappings SELECT @Id, @EntityVersion, @Published, @EnvironmentId, @SitecoreId, @ParentCatalogList, @CatalogToEntityList, @ChildrenCategoryList, @ChildrenSellableItemList, @ParentCategoryList, @IsBundle, @ItemVariations

	IF(@Id LIKE 'Entity-Catalog-%')
	BEGIN
		INSERT INTO [dbo].[Mappings]
		SELECT DISTINCT
			NEWID() as Id
			,@Id as EntityId
			,@EntityVersion as EntityVersion
			,@Published as Published
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
			,@Published as Published
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
			,@Published as Published
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
			,@Published as Published
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
			,@Published as Published
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
			,@Published as Published
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

DROP PROCEDURE IF EXISTS [dbo].[sp_CatalogUpdateMappings];
GO

CREATE PROCEDURE [dbo].[sp_CatalogUpdateMappings]
	@Id NVARCHAR(150),
	@EntityVersion int,
	@Published bit,
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
		Published bit,
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

	INSERT INTO @CatalogMappings SELECT @Id, @EntityVersion, @Published, @EnvironmentId, @SitecoreId, @ParentCatalogList, @CatalogToEntityList, @ChildrenCategoryList, @ChildrenSellableItemList, @ParentCategoryList, @IsBundle, @ItemVariations

	IF(@Id LIKE 'Entity-Catalog-%')
	BEGIN
		INSERT INTO [dbo].[Mappings]
		SELECT DISTINCT
			NEWID() as Id
			,@Id as EntityId
			,@EntityVersion as EntityVersion
			,@Published as Published
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
			,@Published as Published
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
			,@Published as Published
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
			,@Published as Published
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
			,@Published as Published
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
			,@Published as Published
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

DROP PROCEDURE IF EXISTS [dbo].[sp_CommerceEntitiesInsertWithSharding];
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

				EXEC [dbo].[sp_CatalogInsertMappings] @Id, @EntityVersion, @Published, @EnvironmentId, @SitecoreId, @ParentCatalogList, @CatalogToEntityList, @ChildrenCategoryList, @ChildrenSellableItemList, @ParentCategoryList, @IsBundle, @ItemVariations
	END
END
GO

DROP PROCEDURE IF EXISTS [dbo].[sp_CommerceEntitiesUpdateWithSharding];
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

		EXEC [dbo].[sp_CatalogUpdateMappings] @Id, @EntityVersion, @Published, @EnvironmentId, @SitecoreId, @ParentCatalogList, @CatalogToEntityList, @ChildrenCategoryList, @ChildrenSellableItemList, @ParentCategoryList, @IsBundle, @ItemVariations;

	END
END
GO
