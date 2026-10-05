IF DB_NAME()<>N'EnglishCenter_D07_20261005' THROW 51900,'Wrong isolated database',1;
DECLARE @views TABLE(name sysname PRIMARY KEY);
INSERT @views VALUES ('vw_AdmissionsRegistrationFacts'),('vw_EntryTestBookingFacts'),('vw_CurrentScheduleOccurrences'),('vw_TrainingClassFacts'),('vw_FinalizedGradeFacts'),('vw_FinanceInvoiceFacts');
IF EXISTS(SELECT name FROM @views EXCEPT SELECT name FROM sys.views WHERE is_ms_shipped=0)
 OR EXISTS(SELECT name FROM sys.views WHERE is_ms_shipped=0 EXCEPT SELECT name FROM @views)
 THROW 51900,'Exact six view names mismatch',1;
IF EXISTS(SELECT 1 FROM sys.procedures WHERE is_ms_shipped=0)
 OR (SELECT COUNT(*) FROM dbo.__EFMigrationsHistory)<>4
 OR (SELECT COUNT(*) FROM sys.triggers WHERE is_ms_shipped=0)<>5
 OR (SELECT COUNT(*) FROM sys.objects WHERE type IN('FN','IF') AND is_ms_shipped=0)<>6
 OR (SELECT COUNT(*) FROM sys.tables WHERE name<>'__EFMigrationsHistory')<>24
 OR (SELECT COUNT(*) FROM sys.foreign_keys)<>50
 OR (SELECT COUNT(*) FROM sys.check_constraints)<>70
 THROW 51900,'D10 chain/physical baseline mismatch',1;
IF EXISTS(SELECT 1 FROM dbo.Course) OR EXISTS(SELECT 1 FROM dbo.Account)
 OR EXISTS(SELECT 1 FROM dbo.Role) OR EXISTS(SELECT 1 FROM dbo.Band)
 OR EXISTS(SELECT 1 FROM dbo.InvoiceNumberCounter)
 THROW 51900,'Fixture data survived rollback',1;
IF (SELECT compatibility_level FROM sys.databases WHERE database_id=DB_ID())<>160
 OR CONVERT(nvarchar(128),DATABASEPROPERTYEX(DB_NAME(),'Collation'))<>N'Latin1_General_100_CI_AS_SC'
 OR (SELECT snapshot_isolation_state FROM sys.databases WHERE database_id=DB_ID())<>1
 THROW 51900,'Authorized isolated profile mismatch',1;
