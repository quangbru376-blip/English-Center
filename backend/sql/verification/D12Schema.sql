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
 OR (SELECT COUNT(*) FROM dbo.__EFMigrationsHistory)<>6
 OR (SELECT COUNT(*) FROM sys.tables WHERE name<>'__EFMigrationsHistory')<>24
 OR (SELECT COUNT(*) FROM sys.foreign_keys)<>50
 OR (SELECT COUNT(*) FROM sys.check_constraints)<>70
 THROW 51900,'D12 exact object/chain/physical baseline mismatch',1;
IF EXISTS(SELECT 1 FROM dbo.Course) OR EXISTS(SELECT 1 FROM dbo.Account) OR EXISTS(SELECT 1 FROM dbo.Role)
 OR EXISTS(SELECT 1 FROM dbo.Band) OR EXISTS(SELECT 1 FROM dbo.InvoiceNumberCounter)
 THROW 51900,'Fixture data survived rollback',1;
IF (SELECT compatibility_level FROM sys.databases WHERE database_id=DB_ID())<>160
 OR CONVERT(nvarchar(128),DATABASEPROPERTYEX(DB_NAME(),'Collation'))<>N'Latin1_General_100_CI_AS_SC'
 OR (SELECT snapshot_isolation_state FROM sys.databases WHERE database_id=DB_ID())<>1
 THROW 51900,'Authorized isolated profile mismatch',1;
DECLARE @permissions TABLE(roleName sysname,objectName sysname,permissionName nvarchar(128),state char(1),class tinyint,minorId int);
INSERT @permissions VALUES
 ('ec_app_runtime','Role','SELECT','G',1,0),
 ('ec_app_runtime','Band','SELECT','G',1,0),
 ('ec_app_runtime','Account','SELECT','G',1,0),
 ('ec_app_runtime','Account','INSERT','G',1,0),
 ('ec_app_runtime','Account','UPDATE','G',1,0),
 ('ec_app_runtime','StudentProfile','SELECT','G',1,0),
 ('ec_app_runtime','StudentProfile','INSERT','G',1,0),
 ('ec_app_runtime','StudentProfile','UPDATE','G',1,0),
 ('ec_app_runtime','AuthSession','SELECT','G',1,0),
 ('ec_app_runtime','AuthSession','INSERT','G',1,0),
 ('ec_app_runtime','AuthSession','UPDATE','G',1,0),
 ('ec_app_runtime','Course','SELECT','G',1,0),
 ('ec_app_runtime','Course','INSERT','G',1,0),
 ('ec_app_runtime','Course','UPDATE','G',1,0),
 ('ec_app_runtime','TrainingProgram','SELECT','G',1,0),
 ('ec_app_runtime','TrainingProgram','INSERT','G',1,0),
 ('ec_app_runtime','TrainingProgram','UPDATE','G',1,0),
 ('ec_app_runtime','ScoreComponentConfig','SELECT','G',1,0),
 ('ec_app_runtime','ScoreComponentConfig','INSERT','G',1,0),
 ('ec_app_runtime','ScoreComponentConfig','UPDATE','G',1,0),
 ('ec_app_runtime','ScoreComponentConfig','DELETE','G',1,0),
 ('ec_app_runtime','Registration','SELECT','G',1,0),
 ('ec_app_runtime','Registration','INSERT','G',1,0),
 ('ec_app_runtime','Registration','UPDATE','G',1,0),
 ('ec_app_runtime','EntryTestSession','SELECT','G',1,0),
 ('ec_app_runtime','EntryTestSession','INSERT','G',1,0),
 ('ec_app_runtime','EntryTestSession','UPDATE','G',1,0),
 ('ec_app_runtime','EntryTestAttempt','SELECT','G',1,0),
 ('ec_app_runtime','EntryTestAttempt','INSERT','G',1,0),
 ('ec_app_runtime','EntryTestAttempt','UPDATE','G',1,0),
 ('ec_app_runtime','EntryTestBooking','SELECT','G',1,0),
 ('ec_app_runtime','EntryTestBooking','INSERT','G',1,0),
 ('ec_app_runtime','EntryTestBooking','UPDATE','G',1,0),
 ('ec_app_runtime','BandAssignment','SELECT','G',1,0),
 ('ec_app_runtime','BandAssignment','INSERT','G',1,0),
 ('ec_app_runtime','BandAssignment','UPDATE','G',1,0),
 ('ec_app_runtime','Class','SELECT','G',1,0),
 ('ec_app_runtime','Class','INSERT','G',1,0),
 ('ec_app_runtime','Class','UPDATE','G',1,0),
 ('ec_app_runtime','Schedule','SELECT','G',1,0),
 ('ec_app_runtime','Schedule','INSERT','G',1,0),
 ('ec_app_runtime','Schedule','UPDATE','G',1,0),
 ('ec_app_runtime','StudentClassAssignment','SELECT','G',1,0),
 ('ec_app_runtime','StudentClassAssignment','INSERT','G',1,0),
 ('ec_app_runtime','Attendance','SELECT','G',1,0),
 ('ec_app_runtime','Attendance','INSERT','G',1,0),
 ('ec_app_runtime','Attendance','UPDATE','G',1,0),
 ('ec_app_runtime','Score','SELECT','G',1,0),
 ('ec_app_runtime','Score','INSERT','G',1,0),
 ('ec_app_runtime','Score','UPDATE','G',1,0),
 ('ec_app_runtime','GradeReport','SELECT','G',1,0),
 ('ec_app_runtime','GradeReport','INSERT','G',1,0),
 ('ec_app_runtime','GradeReport','UPDATE','G',1,0),
 ('ec_app_runtime','Invoice','SELECT','G',1,0),
 ('ec_app_runtime','Invoice','INSERT','G',1,0),
 ('ec_app_runtime','Invoice','UPDATE','G',1,0),
 ('ec_app_runtime','Payment','SELECT','G',1,0),
 ('ec_app_runtime','Payment','INSERT','G',1,0),
 ('ec_app_runtime','Payment','UPDATE','G',1,0),
 ('ec_app_runtime','AuditLog','SELECT','G',1,0),
 ('ec_app_runtime','AuditLog','INSERT','G',1,0),
 ('ec_app_runtime','ReplayOperation','SELECT','G',1,0),
 ('ec_app_runtime','ReplayOperation','INSERT','G',1,0),
 ('ec_app_runtime','vw_AdmissionsRegistrationFacts','SELECT','G',1,0),
 ('ec_app_runtime','vw_EntryTestBookingFacts','SELECT','G',1,0),
 ('ec_app_runtime','vw_CurrentScheduleOccurrences','SELECT','G',1,0),
 ('ec_app_runtime','vw_TrainingClassFacts','SELECT','G',1,0),
 ('ec_app_runtime','vw_FinalizedGradeFacts','SELECT','G',1,0),
 ('ec_app_runtime','vw_FinanceInvoiceFacts','SELECT','G',1,0),
 ('ec_app_runtime','ufn_BandForScore','SELECT','G',1,0),
 ('ec_app_runtime','ufn_RegistrationUnfinished','SELECT','G',1,0),
 ('ec_app_runtime','ufn_ClassObligationProgress','SELECT','G',1,0),
 ('ec_app_runtime','ufn_GradeTotal','SELECT','G',1,0),
 ('ec_app_runtime','ufn_InvoiceDebtFlags','SELECT','G',1,0),
 ('ec_app_runtime','usp_SearchAdmissions','EXECUTE','G',1,0),
 ('ec_app_runtime','usp_GetEntryTestRoster','EXECUTE','G',1,0),
 ('ec_app_runtime','usp_GetClassGradeReview','EXECUTE','G',1,0),
 ('ec_app_runtime','usp_GetFinancialReport','EXECUTE','G',1,0),
 ('ec_app_runtime','usp_GetAdmissionsReport','EXECUTE','G',1,0),
 ('ec_app_runtime','usp_GetTrainingReport','EXECUTE','G',1,0),
 ('ec_app_runtime','usp_AllocateInvoiceNumber','EXECUTE','G',1,0),
 ('ec_app_runtime','ufn_EffectiveEntryTestState','EXECUTE','G',1,0),
 ('ec_finance_reporting','vw_FinanceInvoiceFacts','SELECT','G',1,0),
 ('ec_finance_reporting','usp_GetFinancialReport','EXECUTE','G',1,0),
 ('ec_academics_reporting','vw_CurrentScheduleOccurrences','SELECT','G',1,0),
 ('ec_academics_reporting','vw_TrainingClassFacts','SELECT','G',1,0),
 ('ec_academics_reporting','usp_GetTrainingReport','EXECUTE','G',1,0),
 ('ec_director_reporting','usp_GetFinancialReport','EXECUTE','G',1,0),
 ('ec_director_reporting','usp_GetAdmissionsReport','EXECUTE','G',1,0),
 ('ec_director_reporting','usp_GetTrainingReport','EXECUTE','G',1,0),
 ('ec_finance_reporting','Account','SELECT','D',1,0),
 ('ec_finance_reporting','StudentProfile','SELECT','D',1,0),
 ('ec_finance_reporting','Score','SELECT','D',1,0),
 ('ec_finance_reporting','GradeReport','SELECT','D',1,0),
 ('ec_academics_reporting','Account','SELECT','D',1,0),
 ('ec_academics_reporting','StudentProfile','SELECT','D',1,0),
 ('ec_academics_reporting','Score','SELECT','D',1,0),
 ('ec_academics_reporting','GradeReport','SELECT','D',1,0),
 ('ec_director_reporting','Account','SELECT','D',1,0),
 ('ec_director_reporting','StudentProfile','SELECT','D',1,0),
 ('ec_director_reporting','Score','SELECT','D',1,0),
 ('ec_director_reporting','GradeReport','SELECT','D',1,0);
DECLARE @actual TABLE(roleName sysname,objectName sysname,permissionName nvarchar(128),state char(1),class tinyint,minorId int);
INSERT @actual SELECT role.name,OBJECT_NAME(p.major_id),p.permission_name,p.state,p.class,p.minor_id
FROM sys.database_permissions p JOIN sys.database_principals role ON role.principal_id=p.grantee_principal_id
WHERE role.name IN ('ec_app_runtime','ec_finance_reporting','ec_academics_reporting','ec_director_reporting');
IF EXISTS(SELECT * FROM @permissions EXCEPT SELECT * FROM @actual)
 OR EXISTS(SELECT * FROM @actual EXCEPT SELECT * FROM @permissions)
 THROW 51912,'Exact D12 object permission matrix mismatch',1;
IF (SELECT COUNT(*) FROM sys.database_principals WHERE name IN ('ec_app_runtime','ec_finance_reporting','ec_academics_reporting','ec_director_reporting') AND type='R' AND is_fixed_role=0 AND owning_principal_id=DATABASE_PRINCIPAL_ID('dbo'))<>4
 THROW 51912,'D12 role ownership/type mismatch',1;
IF EXISTS(SELECT 1 FROM sys.database_role_members m JOIN sys.database_principals p ON p.principal_id=m.role_principal_id OR p.principal_id=m.member_principal_id WHERE p.name IN ('ec_app_runtime','ec_finance_reporting','ec_academics_reporting','ec_director_reporting'))
 OR EXISTS(SELECT 1 FROM sys.database_principals WHERE name LIKE 'D12[_]fixture[_]user[_]%')
 THROW 51912,'D12 test principals/memberships survived rollback',1;
PRINT 'D12 exact4roles/102permissions/6migrations/D11baseline/fixture rollback PASS';
