IF DB_NAME() <> N'EnglishCenter_D07_20261005' THROW 51900,'Wrong isolated database',1;
IF (SELECT COUNT(*) FROM sys.objects WHERE type='FN' AND is_ms_shipped=0)<>1
 OR (SELECT COUNT(*) FROM sys.objects WHERE type='IF' AND is_ms_shipped=0)<>5
 OR EXISTS(SELECT 1 FROM sys.objects WHERE type IN ('TF','P','V') AND is_ms_shipped=0)
 THROW 51900,'UDF types/count or unexpected later objects',1;
DECLARE @expected TABLE(objectName sysname,parameterId int,parameterName sysname,typeName sysname,maxLength smallint,precisionValue tinyint,scaleValue tinyint);
INSERT @expected VALUES
 ('ufn_EffectiveEntryTestState',0,'','varchar',16,0,0),
 ('ufn_EffectiveEntryTestState',1,'@status','varchar',16,0,0),
 ('ufn_EffectiveEntryTestState',2,'@startAt','datetime2',7,23,3),
 ('ufn_EffectiveEntryTestState',3,'@asOfUtc','datetime2',7,23,3),
 ('ufn_BandForScore',1,'@score','decimal',5,5,2),
 ('ufn_RegistrationUnfinished',1,'@registrationId','int',4,10,0),
 ('ufn_ClassObligationProgress',1,'@classId','int',4,10,0),
 ('ufn_GradeTotal',1,'@classAssignmentId','int',4,10,0),
 ('ufn_InvoiceDebtFlags',1,'@status','varchar',16,0,0),
 ('ufn_InvoiceDebtFlags',2,'@dueDate','date',3,10,0),
 ('ufn_InvoiceDebtFlags',3,'@todayVn','date',3,10,0);
IF EXISTS(SELECT * FROM @expected EXCEPT
 SELECT o.name,p.parameter_id,p.name,ty.name,p.max_length,p.precision,p.scale
 FROM sys.objects o JOIN sys.parameters p ON p.object_id=o.object_id JOIN sys.types ty ON ty.user_type_id=p.user_type_id WHERE o.type IN ('FN','IF') AND o.is_ms_shipped=0)
 OR EXISTS(SELECT o.name,p.parameter_id,p.name,ty.name,p.max_length,p.precision,p.scale
 FROM sys.objects o JOIN sys.parameters p ON p.object_id=o.object_id JOIN sys.types ty ON ty.user_type_id=p.user_type_id WHERE o.type IN ('FN','IF') AND o.is_ms_shipped=0
 EXCEPT SELECT * FROM @expected)
 THROW 51900,'Exact UDF parameter/return signature mismatch',1;
IF (SELECT COUNT(*) FROM dbo.__EFMigrationsHistory)<>3
 OR (SELECT COUNT(*) FROM sys.triggers WHERE is_ms_shipped=0)<>5
 OR (SELECT COUNT(*) FROM sys.tables WHERE name<>'__EFMigrationsHistory')<>24
 OR (SELECT COUNT(*) FROM sys.foreign_keys)<>50
 OR (SELECT COUNT(*) FROM sys.check_constraints)<>70
 THROW 51900,'Migration chain/physical baseline mismatch',1;
IF EXISTS(SELECT 1 FROM dbo.Course) OR EXISTS(SELECT 1 FROM dbo.Account)
 OR EXISTS(SELECT 1 FROM dbo.Role) OR EXISTS(SELECT 1 FROM dbo.Band)
 OR EXISTS(SELECT 1 FROM dbo.Score) OR EXISTS(SELECT 1 FROM dbo.Attendance)
 THROW 51900,'Fixture data survived rollback',1;
PRINT 'D09 exact six UDF signatures/types/chain/rollback PASS';
