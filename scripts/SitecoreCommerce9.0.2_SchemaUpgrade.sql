-- This script should run against 'SitecoreCommerce9_SharedEnvironments' and 'SitecoreCommerce9_Global'.

-- This script does the following:
-- 1. Adds new columns to list (EntityVersion) and entity (EntityVersion, Published) tables.
-- 2. Drops all list indexes and recreates a single one that includes the EntityVersion.
-- 3. Updates the primary key constraint in the entity tables.
-- 4. Update stored procedures

DECLARE @AllTableNames TABLE(name VARCHAR(MAX))
INSERT INTO @AllTableNames SELECT name FROM sysobjects WHERE xtype = 'U'

DECLARE @ListTableNames TABLE(name VARCHAR(MAX))
INSERT INTO @ListTableNames SELECT name from @AllTableNames WHERE name LIKE '%Lists'

DECLARE @EntityTableNames TABLE(name VARCHAR(MAX))
INSERT INTO @EntityTableNames SELECT name from @AllTableNames WHERE name LIKE '%Entities'

-- Update list tables
DECLARE @ListTableCursor CURSOR
DECLARE @ListTableName VARCHAR(MAX)
BEGIN
	SET @ListTableCursor = CURSOR FOR SELECT name FROM @ListTableNames
	OPEN @ListTableCursor FETCH NEXT FROM @ListTableCursor INTO @ListTableName

	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE @QuotedListTableName VARCHAR(MAX) = QUOTENAME(@ListTableName)

		-- Add EntityVersion column and populate with initial value
		EXEC('ALTER TABLE ' + @QuotedListTableName + ' ADD EntityVersion INT NULL')
		EXEC('UPDATE ' + @QuotedListTableName + ' SET EntityVersion = 1')

		-- Drop list indexes
		DECLARE @DropIndexQuery VARCHAR(MAX) = 
		(
			SELECT DISTINCT 'DROP INDEX ' + name + ' ON ' + @QuotedListTableName 
			FROM sysindexes 
			WHERE name LIKE ('IX_' + @ListTableName + '%') 
			FOR XML PATH('')
		)

		EXEC(@DropIndexQuery)

		-- Create new list index
		DECLARE @QuotedListTableIndexName VARCHAR(MAX) = QUOTENAME('IX_' + @ListTableName)
		EXEC
		(
			'CREATE UNIQUE NONCLUSTERED INDEX ' + @QuotedListTableIndexName + ' ON ' + @QuotedListTableName + 
			'(' +
				'[EnvironmentId] ASC,' +
				'[ListName] ASC,' +
				'[CommerceEntityId] ASC,' +
				'[EntityVersion] DESC' +
			') WITH (ALLOW_PAGE_LOCKS = OFF)'
		)

		FETCH NEXT FROM @ListTableCursor INTO @ListTableName
	END
END

-- Update entity tables
DECLARE @EntityTableCursor CURSOR
DECLARE @EntityTableName VARCHAR(MAX)
BEGIN
	SET @EntityTableCursor = CURSOR FOR SELECT name FROM @EntityTableNames
	OPEN @EntityTableCursor FETCH NEXT FROM @EntityTableCursor INTO @EntityTableName

	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE @QuotedEntityTableName VARCHAR(MAX) = QUOTENAME(@EntityTableName)

		-- Add EntityVersion and Published columns and populate with default value
		EXEC('ALTER TABLE ' + @QuotedEntityTableName + ' ADD EntityVersion INT NOT NULL DEFAULT 1')
		EXEC('ALTER TABLE ' + @QuotedEntityTableName + ' ADD Published BIT NOT NULL DEFAULT 1')
		EXEC('UPDATE ' + @QuotedEntityTableName + ' SET EntityVersion = 1, Published = 1')

		-- Update primary key constraint
		DECLARE @QuotedEntityTableConstraintName VARCHAR(MAX) = QUOTENAME('PK_' + @EntityTableName)
		EXEC('ALTER TABLE ' + @QuotedEntityTableName + ' DROP CONSTRAINT ' + @QuotedEntityTableConstraintName)
		EXEC('ALTER TABLE ' + @QuotedEntityTableName + ' ADD CONSTRAINT ' + @QuotedEntityTableConstraintName + ' PRIMARY KEY NONCLUSTERED ([EnvironmentId] ASC, [Id] ASC, [EntityVersion] DESC) WITH (ALLOW_PAGE_LOCKS = OFF)')

		FETCH NEXT FROM @EntityTableCursor INTO @EntityTableName
	END
END

-- Update stored procedures
/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesInsert]    Script Date: 2018-06-15 5:30:06 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceEntitiesInsert]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesInsert]    Script Date: 2018-06-15 5:30:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceEntitiesInsert]
(
	@Id nvarchar(150),
	@EnvironmentId uniqueidentifier,
	@Version int,
	@EntityVersion int,
	@Entity nvarchar(max),
	@Published bit
)

as

set nocount on

insert into [CommerceEntities]
(
	[Id],
	[EnvironmentId],
	[Version],
	[EntityVersion],
	[Entity],
	[Published]
)
values
(
	@Id,
	@EnvironmentId,
	@Version,
	@EntityVersion,
	@Entity,
	@Published
)
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesInsertWithSharding]    Script Date: 2018-06-15 5:30:26 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceEntitiesInsertWithSharding]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesInsertWithSharding]    Script Date: 2018-06-15 5:30:26 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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

END
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesSelect]    Script Date: 2018-06-15 5:30:59 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceEntitiesSelect]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesSelect]    Script Date: 2018-06-15 5:30:59 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_CommerceEntitiesSelect]
(
	@Id NVARCHAR(150),
	@EnvironmentId UNIQUEIDENTIFIER,
	@EntityVersion INT = NULL,
	@IgnorePublished BIT = 0
)

AS

SET NOCOUNT ON

SELECT TOP 1
	[Entity]
FROM
	[CommerceEntities]
WITH (NOLOCK)
WHERE
	[EnvironmentId] = @EnvironmentId
	AND
	[Id] = @Id
	AND (((@EntityVersion IS NULL AND [Published] = 1) OR @IgnorePublished = 1) OR [EntityVersion] = @EntityVersion)
ORDER BY
	[EntityVersion] DESC
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesSelectAllVersions]    Script Date: 2018-06-15 5:38:38 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_CommerceEntitiesSelectAllVersions]
(
	@Id NVARCHAR(150),
	@EnvironmentId UNIQUEIDENTIFIER
)

AS

SET NOCOUNT ON

SELECT
	[Entity]
FROM
	[CommerceEntities]
WITH (NOLOCK)
WHERE
	[EnvironmentId] = @EnvironmentId
	AND
	[Id] = @Id
ORDER BY
	[EntityVersion] DESC
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesSelectAllVersionsWithSharding]    Script Date: 2018-06-15 5:38:50 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_CommerceEntitiesSelectAllVersionsWithSharding]
(
	@Id NVARCHAR(150),
	@TableName NVARCHAR(150) = 'CommerceEntities',
	@EnvironmentId UNIQUEIDENTIFIER
)

AS

BEGIN
	SET NOCOUNT ON;

	DECLARE @cmd AS NVARCHAR(MAX);
	DECLARE @parameters AS NVARCHAR(MAX);

	SET @cmd = N'SELECT [Entity] FROM'+ QUOTENAME(OBJECT_NAME(OBJECT_ID(@TableName))) +' WITH (NOLOCK) WHERE [EnvironmentId] = @EnvId AND [Id] = @EntityId ORDER BY [EntityVersion] DESC';
	SET @parameters = '@EnvId UNIQUEIDENTIFIER, @EntityId nvarchar(150)';

	EXEC sp_executesql @cmd, @parameters, @EnvId = @EnvironmentId, @EntityId = @Id;

END
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesSelectWithSharding]    Script Date: 2018-06-15 5:32:08 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceEntitiesSelectWithSharding]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesSelectWithSharding]    Script Date: 2018-06-15 5:32:08 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_CommerceEntitiesSelectWithSharding]
(
	@Id NVARCHAR(150),
	@TableName NVARCHAR(150) = 'CommerceEntities',
	@EnvironmentId UNIQUEIDENTIFIER,
	@EntityVersion INT = NULL,
	@IgnorePublished BIT = 0
)

AS

BEGIN
	SET NOCOUNT ON;

	DECLARE @cmd AS NVARCHAR(MAX);
	DECLARE @parameters AS NVARCHAR(MAX);

	SET @cmd = N'SELECT TOP 1 [Entity] FROM'+ QUOTENAME(OBJECT_NAME(OBJECT_ID(@TableName))) +' WITH (NOLOCK) WHERE [EnvironmentId] = @EnvId AND [Id] = @EntityId AND (((@EntVers IS NULL AND [Published] = 1) OR @IgnorePub = 1) OR [EntityVersion] = @EntVers) ORDER BY [EntityVersion] DESC';
	SET @parameters = '@EnvId UNIQUEIDENTIFIER, @EntityId nvarchar(150), @EntVers INT = NULL, @IgnorePub BIT = 0';

	EXEC sp_executesql @cmd, @parameters, @EnvId = @EnvironmentId, @EntityId = @Id, @EntVers = @EntityVersion, @IgnorePub = @IgnorePublished;

END
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesUpdate]    Script Date: 2018-06-15 5:32:26 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceEntitiesUpdate]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesUpdate]    Script Date: 2018-06-15 5:32:26 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceEntitiesUpdate]
(
	@Id nvarchar(150),
	@EnvironmentId uniqueidentifier,
	@Version int,
	@EntityVersion int,
	@Entity nvarchar(max),
	@Published bit
)

as

set nocount on

--/* If the version supplied is lower or equal to the current version, then we raise a concurrency error */
UPDATE [CommerceEntities]
SET 
       [Entity] = @Entity,
       [Version] = @Version,
	   [Published] = @Published
WHERE 
       [EnvironmentId] = @EnvironmentId
       AND
       [Id] = @Id
       AND
       [Version] = @Version - 1
	   AND
	   [EntityVersion] = @EntityVersion

IF (@@ROWCOUNT=0)
BEGIN
		DECLARE @ErrorMsg NVARCHAR(2048) = FORMATMESSAGE('Concurrency error: The Entity version supplied (%i) is no longer the current version.', @Version);
		THROW 50000, @ErrorMsg, 1;
END
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesUpdateWithSharding]    Script Date: 2018-06-15 5:32:50 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceEntitiesUpdateWithSharding]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesUpdateWithSharding]    Script Date: 2018-06-15 5:32:50 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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
END
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsCount]    Script Date: 2018-06-15 5:33:10 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceListsCount]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsCount]    Script Date: 2018-06-15 5:33:10 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


create procedure [dbo].[sp_CommerceListsCount]
(
	@ListName nvarchar(150),
	@EnvironmentId uniqueidentifier
)

as

set nocount on

select 
	COUNT(DISTINCT CommerceEntityId)
from 
	[CommerceLists]
with (nolock)
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[ListName] = @ListName
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsCountWithSharding]    Script Date: 2018-06-15 5:33:25 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceListsCountWithSharding]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsCountWithSharding]    Script Date: 2018-06-15 5:33:25 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_CommerceListsCountWithSharding]
(
	@ListName NVARCHAR(150),
	@TableName NVARCHAR(150) = 'CommerceLists',
	@EnvironmentId UNIQUEIDENTIFIER
)

AS

BEGIN
	SET NOCOUNT ON

	DECLARE @cmd AS NVARCHAR(MAX)

	SET @cmd = N'SELECT COUNT(DISTINCT CommerceEntityId) FROM '+ QUOTENAME(OBJECT_NAME(OBJECT_ID(@TableName))) +' WITH (NOLOCK) WHERE [EnvironmentId] = @EnvId AND [ListName] = @Name'

	EXEC sp_executesql @cmd, N'@EnvId UNIQUEIDENTIFIER, @Name NVARCHAR(150)', @EnvId = @EnvironmentId, @Name = @ListName
END
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsDeleteEntity]    Script Date: 2018-06-15 5:33:51 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceListsDeleteEntity]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsDeleteEntity]    Script Date: 2018-06-15 5:33:51 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


create procedure [dbo].[sp_CommerceListsDeleteEntity]
(
	@ListName nvarchar(150),
	@EnvironmentId uniqueidentifier,
	@Id nvarchar(150),
	@EntityVersion int = 1
)

as

set nocount on

delete from 
	[CommerceLists]
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[ListName] = @ListName
	AND
	[CommerceEntityId] = @Id
	AND
	[EntityVersion] = @EntityVersion
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsDeleteEntityWithSharding]    Script Date: 2018-06-15 5:34:12 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceListsDeleteEntityWithSharding]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsDeleteEntityWithSharding]    Script Date: 2018-06-15 5:34:12 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_CommerceListsDeleteEntityWithSharding]
(
	@ListName NVARCHAR(150),
	@TableName NVARCHAR(150) = 'CommerceLists',
	@EnvironmentId UNIQUEIDENTIFIER,
	@Id NVARCHAR(150),
	@EntityVersion INT = 1
)

AS

BEGIN
	SET NOCOUNT ON

	DECLARE @cmd AS NVARCHAR(MAX)

	SET @cmd = N'DELETE FROM '+ QUOTENAME(OBJECT_NAME(OBJECT_ID(@TableName))) +' WHERE [EnvironmentId] = @EnvId AND [ListName] = @Name AND [CommerceEntityId] = @EntityId AND [EntityVersion] = @Version'

	EXEC sp_executesql @cmd, N'@EnvId UNIQUEIDENTIFIER, @Name NVARCHAR(150), @EntityId NVARCHAR(150), @Version INT', @EnvId = @EnvironmentId, @Name = @ListName, @EntityId = @Id, @Version = @EntityVersion
END
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsInsert]    Script Date: 2018-06-15 5:34:41 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceListsInsert]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsInsert]    Script Date: 2018-06-15 5:34:41 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


create procedure [dbo].[sp_CommerceListsInsert]
(
	@ListName nvarchar(150),
	@EnvironmentId uniqueidentifier,
	@CommerceEntityId nvarchar(150),
	@EntityVersion INT = 1
)

as

set nocount on

-- If the list entry already exists, do not duplicate it
IF NOT EXISTS (SELECT [ListName], [EnvironmentId],[CommerceEntityId],[EntityVersion] FROM [dbo].[CommerceLists] 
WITH (rowlock)
WHERE [ListName] = @ListName  and [CommerceEntityId] = @CommerceEntityId AND [EnvironmentId] = @EnvironmentId AND [EntityVersion] = @EntityVersion)
BEGIN
	insert into [CommerceLists]
	(
		[ListName],
		[EnvironmentId],
		[CommerceEntityId],
		[EntityVersion]
	)
	values
	(
		@ListName,
		@EnvironmentId,
		@CommerceEntityId,
		@EntityVersion
	)
END
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsInsertWithSharding]    Script Date: 2018-06-15 5:35:02 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceListsInsertWithSharding]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsInsertWithSharding]    Script Date: 2018-06-15 5:35:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_CommerceListsInsertWithSharding]
(
	@ListName NVARCHAR(150),
	@TableName NVARCHAR(150) = 'CommerceLists',
	@EnvironmentId UNIQUEIDENTIFIER,
	@CommerceEntityId NVARCHAR(150),
	@EntityVersion INT = 1
)

AS

BEGIN
	SET NOCOUNT ON;

	DECLARE @cmd AS NVARCHAR(MAX);
	DECLARE @parameters AS NVARCHAR(MAX);

	set @cmd = N'IF NOT EXISTS (SELECT [ListName], [EnvironmentId],[CommerceEntityId],[EntityVersion] FROM '+ QUOTENAME(OBJECT_NAME(OBJECT_ID(@TableName))) +
			' WITH (rowlock) WHERE [ListName] = @Name and [CommerceEntityId] = @EntityId AND [EnvironmentId] = @EnvId AND [EntityVersion] = @Version) BEGIN INSERT INTO '+ QUOTENAME(OBJECT_NAME(OBJECT_ID(@TableName))) +
			' ([ListName],[EnvironmentId],[CommerceEntityId],[EntityVersion]) VALUEs (@Name,@EnvId,@EntityId,@Version) END';
	SET @parameters = '@Name NVARCHAR(150), @EnvId UNIQUEIDENTIFIER, @EntityId NVARCHAR(150), @Version INT';

	EXEC sp_executesql @cmd, @parameters, @Name = @ListName, @EnvId = @EnvironmentId, @EntityId = @CommerceEntityId, @Version = @EntityVersion;
END
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelect]    Script Date: 2018-06-15 5:35:26 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceListsSelect]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelect]    Script Date: 2018-06-15 5:35:26 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


create procedure [dbo].[sp_CommerceListsSelect]
(
	@ListName nvarchar(150),
	@EnvironmentId uniqueidentifier
)

as

set nocount on

select 
	[ListName],
	[EnvironmentId],
	[CommerceEntityId],
	[EntityVersion]
from 
	[CommerceLists]
with (nolock)
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[ListName] = @ListName
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelectAll]    Script Date: 2018-06-15 5:35:39 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceListsSelectAll]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelectAll]    Script Date: 2018-06-15 5:35:39 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


create procedure [dbo].[sp_CommerceListsSelectAll]

as

set nocount on

select 
	[ListName],
	[EnvironmentId],
	[CommerceEntityId],
	[EntityVersion]
from 
	[CommerceLists]
with (nolock)
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelectAllWithSharding]    Script Date: 2018-06-15 5:35:52 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceListsSelectAllWithSharding]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelectAllWithSharding]    Script Date: 2018-06-15 5:35:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_CommerceListsSelectAllWithSharding]

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

	OPEN listTableCursor
	FETCH NEXT FROM listTableCursor INTO @listTableName

	WHILE @@FETCH_STATUS = 0
	BEGIN 
		DECLARE @cmd AS NVARCHAR(max)

		SET @cmd = N'SELECT [ListName],[EnvironmentId],[CommerceEntityId],[EntityVersion] INTO ##TempTable FROM @Name WITH (NOLOCK)'

		EXEC sp_executesql @cmd, N'@Name NVARCHAR(150)', @Name = @listTableName 
		FETCH NEXT FROM listTableCursor INTO @listTableName
	END

	select 
		[ListName],
		[EnvironmentId],
		[CommerceEntityId],
		[EntityVersion]
	from 
		##TempTable
	with (nolock)

END
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelectByRange]    Script Date: 2018-06-15 5:36:11 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceListsSelectByRange]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelectByRange]    Script Date: 2018-06-15 5:36:11 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


create procedure [dbo].[sp_CommerceListsSelectByRange]
(
	@ListName nvarchar(150),
	@EnvironmentId uniqueidentifier,
	@Skip int = 0,
	@Take int = 2,
	@SortOrder int = 0
)

as

set nocount on

SELECT 
	[CommerceEntityId],
	MAX([EntityVersion])
FROM CommerceLists
WITH (NOLOCK)
WHERE 
	[EnvironmentId] = @EnvironmentId
	AND
	[ListName] = @ListName
GROUP BY
	[CommerceEntityId]
ORDER BY 
CASE WHEN @SortOrder = 0  THEN
	[CommerceEntityId] END ASC, 
CASE WHEN @SortOrder = 1 THEN 
	[CommerceEntityId] END DESC
	OFFSET @Skip ROWS
FETCH NEXT @Take ROWS ONLY
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelectByRangeWithSharding]    Script Date: 2018-06-15 5:36:32 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceListsSelectByRangeWithSharding]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelectByRangeWithSharding]    Script Date: 2018-06-15 5:36:32 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_CommerceListsSelectByRangeWithSharding]
(
	@ListName NVARCHAR(150),
	@TableName NVARCHAR(150) = 'CommerceLists',
	@EnvironmentId UNIQUEIDENTIFIER,
	@Skip INT = 0,
	@Take INT = 2,
	@SortOrder INT = 0
)

AS

BEGIN
	SET NOCOUNT ON

	DECLARE @cmd AS NVARCHAR(MAX)
    
	SET @cmd = N'SELECT [CommerceEntityId], MAX([EntityVersion]) FROM '+ QUOTENAME(OBJECT_NAME(OBJECT_ID(@TableName))) +' WITH (NOLOCK) WHERE [EnvironmentId] = @EnvId  AND [ListName] = @Name GROUP BY [CommerceEntityId] ORDER BY CASE WHEN @SO = 0 THEN [CommerceEntityId] END ASC, CASE WHEN @SO = 1 THEN [CommerceEntityId] END DESC OFFSET @SK ROWS FETCH NEXT @TK ROWS ONLY'

	EXEC sp_executesql @cmd, N'@EnvId UNIQUEIDENTIFIER, @Name NVARCHAR(150), @SO INT, @SK INT, @TK INT', @EnvId = @EnvironmentId, @Name = @ListName, @SO = @SortOrder, @SK = @Skip, @TK = @Take
END
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelectWithSharding]    Script Date: 2018-06-15 5:36:55 PM ******/
DROP PROCEDURE [dbo].[sp_CommerceListsSelectWithSharding]
GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelectWithSharding]    Script Date: 2018-06-15 5:36:55 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_CommerceListsSelectWithSharding]
(
	@ListName NVARCHAR(150),
	@TableName NVARCHAR(150) = 'CommerceLists',
	@EnvironmentId UNIQUEIDENTIFIER
)

AS

BEGIN
	SET NOCOUNT ON

	DECLARE @cmd AS NVARCHAR(max)

	SET @cmd = N'SELECT [ListName],[EnvironmentId],[CommerceEntityId],[EntityVersion] FROM '+ QUOTENAME(OBJECT_NAME(OBJECT_ID(@TableName))) +' WITH (NOLOCK) WHERE [EnvironmentId] = @EnvId AND [ListName] = @Name'

	EXEC sp_executesql @cmd, N'@EnvId UNIQUEIDENTIFIER, @Name NVARCHAR(150)', @EnvId = @EnvironmentId, @Name = @ListName
END
GO