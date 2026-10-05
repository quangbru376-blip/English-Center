IF DB_NAME()<>N'EnglishCenter_D07_20261005' THROW 51900,'Wrong isolated database',1;
DECLARE @expected TABLE(objectName sysname,parameterId int,parameterName sysname,typeName sysname,maxLength smallint,precisionValue tinyint,scaleValue tinyint);
INSERT @expected VALUES
 ('usp_SearchAdmissions',1,'@actorId','int',4,10,0),
 ('usp_SearchAdmissions',2,'@courseId','int',4,10,0),
 ('usp_SearchAdmissions',3,'@status','varchar',32,0,0),
 ('usp_SearchAdmissions',4,'@createdFromUtc','datetime2',7,23,3),
 ('usp_SearchAdmissions',5,'@createdToExclusiveUtc','datetime2',7,23,3),
 ('usp_SearchAdmissions',6,'@name','nvarchar',400,0,0),
 ('usp_SearchAdmissions',7,'@email','varchar',254,0,0),
 ('usp_SearchAdmissions',8,'@phone','varchar',10,0,0),
 ('usp_SearchAdmissions',9,'@sort','varchar',16,0,0),
 ('usp_SearchAdmissions',10,'@page','int',4,10,0),
 ('usp_SearchAdmissions',11,'@pageSize','int',4,10,0),
 ('usp_GetEntryTestRoster',1,'@sessionId','int',4,10,0),
 ('usp_GetEntryTestRoster',2,'@actorId','int',4,10,0),
 ('usp_GetEntryTestRoster',3,'@asOfUtc','datetime2',7,23,3),
 ('usp_GetEntryTestRoster',4,'@name','nvarchar',400,0,0),
 ('usp_GetEntryTestRoster',5,'@email','varchar',254,0,0),
 ('usp_GetEntryTestRoster',6,'@phone','varchar',10,0,0),
 ('usp_GetEntryTestRoster',7,'@bookingStatus','varchar',16,0,0),
 ('usp_GetEntryTestRoster',8,'@view','varchar',8,0,0),
 ('usp_GetEntryTestRoster',9,'@sort','varchar',16,0,0),
 ('usp_GetEntryTestRoster',10,'@page','int',4,10,0),
 ('usp_GetEntryTestRoster',11,'@pageSize','int',4,10,0),
 ('usp_GetClassGradeReview',1,'@classId','int',4,10,0),
 ('usp_GetClassGradeReview',2,'@actorId','int',4,10,0),
 ('usp_GetFinancialReport',1,'@fromUtc','datetime2',7,23,3),
 ('usp_GetFinancialReport',2,'@toExclusiveUtc','datetime2',7,23,3),
 ('usp_GetFinancialReport',3,'@courseId','int',4,10,0),
 ('usp_GetFinancialReport',4,'@overdueOnly','bit',1,1,0),
 ('usp_GetFinancialReport',5,'@todayVn','date',3,10,0),
 ('usp_GetFinancialReport',6,'@page','int',4,10,0),
 ('usp_GetFinancialReport',7,'@pageSize','int',4,10,0),
 ('usp_GetAdmissionsReport',1,'@fromUtc','datetime2',7,23,3),
 ('usp_GetAdmissionsReport',2,'@toExclusiveUtc','datetime2',7,23,3),
 ('usp_GetAdmissionsReport',3,'@courseId','int',4,10,0),
 ('usp_GetAdmissionsReport',4,'@status','varchar',16,0,0),
 ('usp_GetAdmissionsReport',5,'@page','int',4,10,0),
 ('usp_GetAdmissionsReport',6,'@pageSize','int',4,10,0),
 ('usp_GetTrainingReport',1,'@courseId','int',4,10,0),
 ('usp_GetTrainingReport',2,'@bandId','int',4,10,0),
 ('usp_GetTrainingReport',3,'@classId','int',4,10,0),
 ('usp_GetTrainingReport',4,'@teacherId','int',4,10,0),
 ('usp_GetTrainingReport',5,'@status','varchar',16,0,0),
 ('usp_GetTrainingReport',6,'@finalizedFromUtc','datetime2',7,23,3),
 ('usp_GetTrainingReport',7,'@finalizedToExclusiveUtc','datetime2',7,23,3),
 ('usp_GetTrainingReport',8,'@page','int',4,10,0),
 ('usp_GetTrainingReport',9,'@pageSize','int',4,10,0),
 ('usp_AllocateInvoiceNumber',1,'@issueYear','smallint',2,5,0);
IF EXISTS(SELECT * FROM @expected EXCEPT
 SELECT o.name,p.parameter_id,p.name,ty.name,p.max_length,p.precision,p.scale FROM sys.procedures o
 JOIN sys.parameters p ON p.object_id=o.object_id JOIN sys.types ty ON ty.user_type_id=p.user_type_id WHERE o.is_ms_shipped=0)
 OR EXISTS(SELECT o.name,p.parameter_id,p.name,ty.name,p.max_length,p.precision,p.scale FROM sys.procedures o
 JOIN sys.parameters p ON p.object_id=o.object_id JOIN sys.types ty ON ty.user_type_id=p.user_type_id WHERE o.is_ms_shipped=0
 EXCEPT SELECT * FROM @expected)
 THROW 51900,'Exact seven procedure parameter signatures mismatch',1;
IF (SELECT COUNT(*) FROM sys.procedures WHERE is_ms_shipped=0)<>7
 OR (SELECT COUNT(*) FROM sys.views WHERE is_ms_shipped=0)<>6
 OR (SELECT COUNT(*) FROM sys.objects WHERE type IN('FN','IF') AND is_ms_shipped=0)<>6
 OR (SELECT COUNT(*) FROM sys.triggers WHERE is_ms_shipped=0 AND is_disabled=0)<>5
 OR (SELECT COUNT(*) FROM dbo.__EFMigrationsHistory)<>5
 OR (SELECT COUNT(*) FROM sys.tables WHERE name<>'__EFMigrationsHistory')<>24
 OR (SELECT COUNT(*) FROM sys.foreign_keys)<>50
 OR (SELECT COUNT(*) FROM sys.check_constraints)<>70
 THROW 51900,'D11 exact object/chain/physical baseline mismatch',1;
IF EXISTS(SELECT 1 FROM dbo.Course) OR EXISTS(SELECT 1 FROM dbo.Account) OR EXISTS(SELECT 1 FROM dbo.Role)
 OR EXISTS(SELECT 1 FROM dbo.Band) OR EXISTS(SELECT 1 FROM dbo.InvoiceNumberCounter)
 THROW 51900,'Fixture data survived rollback',1;
IF (SELECT compatibility_level FROM sys.databases WHERE database_id=DB_ID())<>160
 OR CONVERT(nvarchar(128),DATABASEPROPERTYEX(DB_NAME(),'Collation'))<>N'Latin1_General_100_CI_AS_SC'
 OR (SELECT snapshot_isolation_state FROM sys.databases WHERE database_id=DB_ID())<>1
 THROW 51900,'Authorized isolated profile mismatch',1;
PRINT 'D11 exact47parameters/7SP/6VW/6FN/5TG/24tables/5migrations/rollback PASS';
