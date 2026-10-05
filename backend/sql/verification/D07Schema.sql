IF DB_NAME() <> N'EnglishCenter_D07_20261005' THROW 51000,'Wrong isolated database',1;
IF (SELECT compatibility_level FROM sys.databases WHERE name=DB_NAME()) <> 160
 OR (SELECT collation_name FROM sys.databases WHERE name=DB_NAME()) <> N'Latin1_General_100_CI_AS_SC'
 OR (SELECT snapshot_isolation_state FROM sys.databases WHERE name=DB_NAME()) <> 1
 THROW 51000,'Database profile mismatch',1;
IF (SELECT COUNT(*) FROM sys.tables WHERE name <> '__EFMigrationsHistory') <> 24
 OR (SELECT COUNT(*) FROM sys.columns c JOIN sys.tables t ON t.object_id=c.object_id WHERE t.name <> '__EFMigrationsHistory') <> 230
 OR (SELECT COUNT(*) FROM sys.key_constraints WHERE type='PK' AND parent_object_id <> OBJECT_ID('dbo.__EFMigrationsHistory')) <> 24
 OR (SELECT COUNT(*) FROM sys.key_constraints WHERE type='UQ') <> 2
 OR (SELECT COUNT(*) FROM sys.foreign_keys) <> 50
 OR (SELECT COUNT(*) FROM sys.check_constraints) <> 70
 OR (SELECT COUNT(*) FROM sys.indexes i JOIN sys.tables t ON t.object_id=i.object_id WHERE i.index_id>0 AND i.is_primary_key=0 AND i.is_unique_constraint=0 AND t.name <> '__EFMigrationsHistory') <> 78
 OR (SELECT COUNT(*) FROM sys.columns WHERE system_type_id=189) <> 20
 THROW 51000,'Physical schema signature mismatch',1;
IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE delete_referential_action<>0 OR update_referential_action<>0 OR is_not_trusted=1 OR is_disabled=1)
 OR EXISTS(SELECT 1 FROM sys.check_constraints WHERE is_not_trusted=1 OR is_disabled=1)
 THROW 51000,'Constraint action/trust mismatch',1;
IF EXISTS(SELECT 1 FROM sys.triggers WHERE parent_class=1 AND is_ms_shipped=0)
 OR EXISTS(SELECT 1 FROM sys.objects WHERE type IN ('FN','IF','TF','V','P') AND is_ms_shipped=0)
 THROW 51000,'Unexpected objects before D08/D09',1;
SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
BEGIN TRANSACTION;
SELECT COUNT(*) AS migrationCount FROM dbo.__EFMigrationsHistory;
ROLLBACK;
PRINT 'D07 isolated schema/profile/snapshot PASS';
