SET NOCOUNT ON;
IF DB_NAME() <> N'EnglishCenter_D07_20261005' THROW 51901, 'Wrong designated database', 1;
IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name LIKE N'D12_fixture_user_%') THROW 51902, 'Retained fixture user', 1;
IF EXISTS (SELECT 1 FROM sys.database_role_members m JOIN sys.database_principals r ON r.principal_id=m.role_principal_id WHERE r.name LIKE N'ec[_]%') THROW 51903, 'Unexpected role member', 1;
IF (SELECT COUNT(*) FROM sys.database_principals WHERE name LIKE N'ec[_]%' AND type='R')<>4 THROW 51909, 'Role baseline drift', 1;
IF (SELECT COUNT(*) FROM sys.database_permissions p JOIN sys.database_principals r ON r.principal_id=p.grantee_principal_id WHERE r.name LIKE N'ec[_]%')<>102 THROW 51910, 'Permission baseline drift', 1;
DECLARE @counts TABLE (name sysname, rows bigint);
DECLARE @name sysname, @query nvarchar(max);
DECLARE tables_cursor CURSOR LOCAL FAST_FORWARD FOR
 SELECT name FROM sys.tables WHERE schema_id=SCHEMA_ID(N'dbo') AND name<>N'__EFMigrationsHistory' ORDER BY name;
OPEN tables_cursor;
FETCH NEXT FROM tables_cursor INTO @name;
WHILE @@FETCH_STATUS=0
BEGIN
 SET @query=N'SELECT N'+QUOTENAME(@name,'''')+N', COUNT_BIG(*) FROM dbo.'+QUOTENAME(@name)+N';';
 INSERT @counts EXEC sys.sp_executesql @query;
 FETCH NEXT FROM tables_cursor INTO @name;
END;
CLOSE tables_cursor;
DEALLOCATE tables_cursor;
IF EXISTS (SELECT 1 FROM @counts WHERE rows<>0) THROW 51904, 'Retained business data', 1;
IF (SELECT COUNT(*) FROM @counts)<>24 THROW 51905, 'Table baseline drift', 1;
IF (SELECT COUNT(*) FROM dbo.__EFMigrationsHistory)<>6 THROW 51906, 'Migration baseline drift', 1;
IF (SELECT compatibility_level FROM sys.databases WHERE database_id=DB_ID())<>160 THROW 51907, 'Compatibility drift', 1;
IF (SELECT snapshot_isolation_state FROM sys.databases WHERE database_id=DB_ID())<>1 THROW 51908, 'Snapshot drift', 1;
DECLARE @snapshot nvarchar(max)=(SELECT
 (SELECT name,compatibility_level,collation_name,snapshot_isolation_state,is_read_committed_snapshot_on FROM sys.databases WHERE database_id=DB_ID() FOR JSON PATH) AS profile,
 (SELECT name,rows FROM @counts ORDER BY name FOR JSON PATH) AS retainedRows,
 (SELECT MigrationId,ProductVersion FROM dbo.__EFMigrationsHistory ORDER BY MigrationId FOR JSON PATH) AS migrations,
 (SELECT name,type_desc FROM sys.database_principals WHERE name LIKE N'ec[_]%' ORDER BY name FOR JSON PATH) AS roles,
 (SELECT p.class,p.major_id,p.minor_id,p.grantee_principal_id,p.grantor_principal_id,p.type,p.state FROM sys.database_permissions p JOIN sys.database_principals r ON r.principal_id=p.grantee_principal_id WHERE r.name LIKE N'ec[_]%' ORDER BY p.grantee_principal_id,p.class,p.major_id,p.minor_id,p.type FOR JSON PATH) AS permissions,
 (SELECT o.object_id,o.name,o.type,o.schema_id,m.definition FROM sys.objects o LEFT JOIN sys.sql_modules m ON m.object_id=o.object_id WHERE o.is_ms_shipped=0 ORDER BY o.object_id FOR JSON PATH) AS objects,
 (SELECT c.* FROM sys.columns c JOIN sys.objects o ON o.object_id=c.object_id WHERE o.is_ms_shipped=0 ORDER BY c.object_id,c.column_id FOR JSON PATH) AS columns,
 (SELECT i.* FROM sys.indexes i JOIN sys.objects o ON o.object_id=i.object_id WHERE o.is_ms_shipped=0 ORDER BY i.object_id,i.index_id FOR JSON PATH) AS indexes,
 (SELECT i.* FROM sys.index_columns i JOIN sys.objects o ON o.object_id=i.object_id WHERE o.is_ms_shipped=0 ORDER BY i.object_id,i.index_id,i.index_column_id FOR JSON PATH) AS indexColumns,
 (SELECT f.* FROM sys.foreign_keys f WHERE f.is_ms_shipped=0 ORDER BY f.object_id FOR JSON PATH) AS foreignKeys,
 (SELECT f.* FROM sys.foreign_key_columns f ORDER BY f.constraint_object_id,f.constraint_column_id FOR JSON PATH) AS foreignKeyColumns,
 (SELECT c.* FROM sys.check_constraints c WHERE c.is_ms_shipped=0 ORDER BY c.object_id FOR JSON PATH) AS checks,
 (SELECT c.* FROM sys.default_constraints c WHERE c.is_ms_shipped=0 ORDER BY c.object_id FOR JSON PATH) AS defaults
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
SELECT CONVERT(varchar(64),HASHBYTES('SHA2_256',@snapshot),2) AS baseline_sha256;
SELECT COUNT(*) AS tables, SUM(rows) AS retained_rows FROM @counts;
SELECT COUNT(*) AS role_permissions FROM sys.database_permissions p JOIN sys.database_principals r ON r.principal_id=p.grantee_principal_id WHERE r.name LIKE N'ec[_]%';
