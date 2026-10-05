# D11 / SP01 — usp_SearchAdmissions

Read COMPLETE this card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md first. Read ONLY these3paths and exact output if it exists. Do not follow links/read AGENTS/other source/memory. No commands/search/scans/network/SQL/Git/MCP/browser/subagents/secrets. Only view_file/write_to_file/replace_file_content. Codex independently tests/reviews/applies. SQLServer160/exact decimal/time/NULL/full-set/current role/privacy E03/E04 override stale SQLite/API/seed rules. No packages/grants/GO/USE/migration/GETDATE or security/profile outputs. One CREATE OR ALTER PROCEDURE; SET NOCOUNT ON; TRY...CATCH THROW. Explicit projections, typed NULL, bigint counts and checked bigint offset=(CONVERT(bigint,@page)-1)*@pageSize. Values parameters only, literal contains LIKE escape '~' then '%' '_' '[' (REPLACE nested), no user SQL/sort concatenation. Null pair bounds follow canonical. No business writes except SP07 counter. No silent truncation, fake0, guessed seed, audit or replay.

Manager SQL integration choices: SP01–06 REQUIRE existing caller transaction with isolation_level=5 (SNAPSHOT) and XACT_STATE()=1. If absent/wrong isolation THROW51104. Never begin/commit/rollback caller transaction. This preserves service-owned transaction plus same-snapshot parent EF reads/options. Read SPs may use table variables/temp staging only, no business DML. Error51101 unauthorized active role/restricted account;51102 scope miss (parent absent/Teacher not assigned/no visible grade report) maps404 in future service;51103 invalid filters/page/date/sort; SP07 absent transaction51107/year/overflow51108. TRY/CATCH THROW without own rollback. Current account active AND must_change_password=0 for role defenses; role_code exact accepted case. SP04–06 no actor parameter or web auth claim (service future).

Exact output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D11/SP01.sql

Accepted exact signature/result contract:
dbo.usp_SearchAdmissions → API-038

Parameters (tất cả explicit): actorId int N; courseId int Y; status varchar(32)
Y; createdFromUtc/createdToExclusiveUtc datetime2(3) Y mỗi field;
name nvarchar(200) Y; email varchar(254) Y; phone varchar(10) Y;
sort varchar(16) N CREATED_ASC/CREATED_DESC; page/pageSize int N.
Actor hiện Admissions ACTIVE được Service kiểm; procedure defense recheck role,
không expose password/status/security context columns.

RS1 một row totalCount bigint N kể cả0. RS2 trang một row/Registration:
registrationId int N, studentId int N, fullName nvarchar(200) N,
email varchar(254) N, phone varchar(10) N,
courseId int N, courseCode varchar(30) N, courseName nvarchar(300) N,
courseStatus E03(T05.status) N,
registrationStatus E03(T09.status) N, createdAt datetime2(3) N,
admissionConfirmedAt/admissionApprovedAt datetime2(3) Y,
bandId int Y, bandCode E03(T06.band_code) Y, bandMinScore decimal(5,2) Y,
invoiceId int Y, invoiceNumber E03(T20.invoice_number) Y,
invoiceAmount decimal(18,0) Y, invoiceStatus E03(T20.status) Y,
classId/assignmentId int Y, classCode varchar(30) Y,
classStatus E03(T14.status) Y, registrationVersion binary(8) N.
Nullable joined bundles all-null hoặc complete; page/count cùng snapshot.
Stable createdAt/id cùng chiều; Service map AdmissionsRegistrationSummary,
không add Payment/StudentProfile/academic grade columns.


Manager integration requirements:
Actor must be ACTIVE normal Admissions. Date bounds optional independently; status enum all7; page>=1,size1..100; sortCREATED_ASC/DESC. Build same filtered registration set; name/email/phone literal contains case-insensitive DB collation. Never require joined band/invoice/class. Explicit RS2 exact26 columns in listed order; registrationVersion CONVERT(binary(8),rowversion). Direction includes ID same direction. No DOB/address/password/payment or grades.


Referenced view projection contract (views generated separately):
backend/src/EnglishCenter.Api/Infrastructure/Sql/D10/VW01.sql

One row/Registration. Inner join its StudentProfile/Account/Course; left join unique BandAssignment/Band, Invoice, Assignment/Class. Exact aliases: registrationId,studentId,fullName,email,phone,courseId,courseCode,courseName,courseStatus,registrationStatus,createdAt,admissionConfirmedAt,admissionApprovedAt,bandId,bandCode,bandMinScore,invoiceId,invoiceNumber,invoiceAmount,invoiceStatus,classId,assignmentId,classCode,classStatus,registrationVersion. Source current Account full_name/email/phone; registration confirmation/approval timestamps; version its row_version. No DOB/address/password/profile snapshot. All nullable joined bundles reflect real absence; do not coalesce. Include every registration status/current course status. Unique joins preserve grain.

Exact initial schema:

CREATE TABLE [dbo].[Account] (
    [account_id] int NOT NULL IDENTITY,
    [role_id] int NOT NULL,
    [email] varchar(254) COLLATE Latin1_General_100_CI_AS_SC NOT NULL,
    [phone] varchar(10) NULL,
    [password_hash] varchar(512) NOT NULL,
    [full_name] nvarchar(200) NOT NULL,
    [status] varchar(16) NOT NULL DEFAULT 'ACTIVE',
    [created_at] datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME()),
    [security_version] bigint NOT NULL DEFAULT CAST(1 AS bigint),
    [failed_login_count] int NOT NULL DEFAULT 0,
    [failure_window_started_at] datetime2(3) NULL,
    [login_throttled_until] datetime2(3) NULL,
    [must_change_password] bit NOT NULL,
    [temp_issued_at] datetime2(3) NULL,
    [temp_expires_at] datetime2(3) NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_Account] PRIMARY KEY ([account_id]),
    CONSTRAINT [CK_Account_FailedLoginCount] CHECK ([failed_login_count] >= 0),
    CONSTRAINT [CK_Account_Phone] CHECK ([phone] IS NULL OR (DATALENGTH([phone]) = 10 AND LEFT([phone], 1) = '0' AND [phone] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^0-9]%')),
    CONSTRAINT [CK_Account_SecurityVersion] CHECK ([security_version] > 0),
    CONSTRAINT [CK_Account_Status] CHECK ([status] COLLATE Latin1_General_100_BIN2 IN ('ACTIVE', 'LOCKED', 'DISABLED') AND DATALENGTH([status]) = LEN([status])),
    CONSTRAINT [CK_Account_TempBundle] CHECK (([must_change_password] = 1 AND [temp_issued_at] IS NOT NULL AND [temp_expires_at] IS NOT NULL AND [temp_expires_at] = DATEADD(hour, 24, [temp_issued_at])) OR ([must_change_password] = 0 AND [temp_issued_at] IS NULL AND [temp_expires_at] IS NULL)),
    CONSTRAINT [FK_Account_Role_role_id] FOREIGN KEY ([role_id]) REFERENCES [dbo].[Role] ([role_id])
);

CREATE TABLE [dbo].[Role] (
    [role_id] int NOT NULL IDENTITY,
    [role_code] varchar(32) NOT NULL,
    [role_name] nvarchar(200) NOT NULL,
    CONSTRAINT [PK_Role] PRIMARY KEY ([role_id]),
    CONSTRAINT [CK_Role_Code] CHECK ([role_code] COLLATE Latin1_General_100_BIN2 IN ('Student', 'Admissions', 'AcademicAffairs', 'Teacher', 'Accountant', 'Director', 'Admin') AND DATALENGTH([role_code]) = LEN([role_code]))
);