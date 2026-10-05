# D11 / SP02 — usp_GetEntryTestRoster

Read COMPLETE this card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md first. Read ONLY these3paths and exact output if it exists. Do not follow links/read AGENTS/other source/memory. No commands/search/scans/network/SQL/Git/MCP/browser/subagents/secrets. Only view_file/write_to_file/replace_file_content. Codex independently tests/reviews/applies. SQLServer160/exact decimal/time/NULL/full-set/current role/privacy E03/E04 override stale SQLite/API/seed rules. No packages/grants/GO/USE/migration/GETDATE or security/profile outputs. One CREATE OR ALTER PROCEDURE; SET NOCOUNT ON; TRY...CATCH THROW. Explicit projections, typed NULL, bigint counts and checked bigint offset=(CONVERT(bigint,@page)-1)*@pageSize. Values parameters only, literal contains LIKE escape '~' then '%' '_' '[' (REPLACE nested), no user SQL/sort concatenation. Null pair bounds follow canonical. No business writes except SP07 counter. No silent truncation, fake0, guessed seed, audit or replay.

Manager SQL integration choices: SP01–06 REQUIRE existing caller transaction with isolation_level=5 (SNAPSHOT) and XACT_STATE()=1. If absent/wrong isolation THROW51104. Never begin/commit/rollback caller transaction. This preserves service-owned transaction plus same-snapshot parent EF reads/options. Read SPs may use table variables/temp staging only, no business DML. Error51101 unauthorized active role/restricted account;51102 scope miss (parent absent/Teacher not assigned/no visible grade report) maps404 in future service;51103 invalid filters/page/date/sort; SP07 absent transaction51107/year/overflow51108. TRY/CATCH THROW without own rollback. Current account active AND must_change_password=0 for role defenses; role_code exact accepted case. SP04–06 no actor parameter or web auth claim (service future).

Exact output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D11/SP02.sql

Accepted exact signature/result contract:
dbo.usp_GetEntryTestRoster → API-065/068

Parameters: sessionId int N, actorId int N, asOfUtc datetime2(3) N;
name nvarchar(200) Y, email varchar(254) Y, phone varchar(10) Y;
bookingStatus varchar(16) Y; view varchar(8) N current/history;
sort varchar(16) N BOOKED_ASC/BOOKED_DESC; page/pageSize int N.
Procedure resolves actual Account role AcademicAffairs/Teacher, Teacher current
Session.grader_id; not passed callerRole. No Session/out-of-task returns tagged
scope miss via service mapping404, **không** empty success pretending valid scope.

RS1 totalCount bigint N; RS2 one scoped Booking row:
bookingId/attemptId/registrationId/studentId/courseId int N;
attemptNumber int N; bookedAt datetime2(3) N;
bookingStatus varchar(16) N, registrationStatus E03(T09.status) N;
fullName nvarchar(200) N, email varchar(254) N, phone varchar(10) N;
courseCode varchar(30) N, courseName nvarchar(300) N,
courseStatus E03(T05.status) N;
targetScore decimal(5,2) Y, targetGradedAt datetime2(3) Y;
bandId int Y, bandCode E03(T06.band_code) Y, bandMinScore decimal(5,2) Y.
AcademicAffairs RS2 thêm invoiceStatus E03(T20.status) Y;
Teacher RS2 **không column invoiceStatus**, không NULL placeholder rights union.
Two keyless row shapes dispatch verified role. targetScore chỉ Booking COMPLETED
và Attempt.completed_booking_id=bookingId; current Band separate, no grade0.
Stable bookedAt/bookingId; history includes current SCHEDULED. FN01 uses same
asOfUtc trong scoped parent context query (không thêm effective state row DTO).


Manager integration requirements:
Actor AcademicAffairs OR current assigned Teacher active normal. Parent missing/wrong assignment THROW51102 (Teacher cannot distinguish outside scope); other role51101. Session cancellation/closed alone does not hide retained history. Evaluate FN01(@status,@startAt,@asOfUtc) once for scoped parent, do not return extra state columns. current=onlySCHEDULED; history=allinclSCHEDULED. bookingStatus conjunctive optional. Validate view/sort/pages/status. Target score bundle already projected view. Exact RS2 branch Teachers omit invoiceStatus entirely, AA append invoiceStatus. Full role variant metadata, not NULL union. Literal search. Count/page same filtered scoped set.


Referenced view projection contract (views generated separately):
backend/src/EnglishCenter.Api/Infrastructure/Sql/D10/VW02.sql

One row/Booking. Join Attempt,Session,Registration,StudentProfile/Account,Course; left unique BandAssignment/Band and Invoice. Aliases: bookingId,attemptId,sessionId,graderId,sessionStatus,sessionStartAt,attemptNumber,registrationId,studentId,courseId,bookedAt,bookingStatus,registrationStatus,fullName,email,phone,courseCode,courseName,courseStatus,targetScore,targetGradedAt,bandId,bandCode,bandMinScore,invoiceStatus. targetScore/targetGradedAt only WHEN booking.status=COMPLETED AND attempt.completed_booking_id=booking.booking_id else typed NULL. Current registration Band separately, not attempt guessed Band. Include SCHEDULED/ABSENT/cancelled/history, no score0. Include session status/start so SP02 can call FN01 with passed time. No password/profile fields.

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

CREATE TABLE [dbo].[EntryTestSession] (
    [session_id] int NOT NULL IDENTITY,
    [start_at] datetime2(3) NOT NULL,
    [end_at] datetime2(3) NOT NULL,
    [room] varchar(30) NOT NULL,
    [capacity] int NOT NULL,
    [grader_id] int NOT NULL,
    [status] varchar(16) NOT NULL DEFAULT 'CLOSED',
    [cancelled_by] int NULL,
    [cancelled_at] datetime2(3) NULL,
    [cancellation_reason] nvarchar(2000) NULL,
    [cancellation_operation_id] uniqueidentifier NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_EntryTestSession] PRIMARY KEY ([session_id]),
    CONSTRAINT [CK_EntryTestSession_CancelledBundle] CHECK (([status] = 'CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancellation_reason] IS NOT NULL AND LEN([cancellation_reason]) > 0 AND [cancellation_operation_id] IS NOT NULL) OR ([status] <> 'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancellation_operation_id] IS NULL)),
    CONSTRAINT [CK_EntryTestSession_Capacity] CHECK ([capacity] BETWEEN 1 AND 20),
    CONSTRAINT [CK_EntryTestSession_Room] CHECK (DATALENGTH([room]) BETWEEN 1 AND 30 AND DATALENGTH([room]) = LEN([room]) AND [room] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z0-9_-]%'),
    CONSTRAINT [CK_EntryTestSession_Status] CHECK ([status] COLLATE Latin1_General_100_BIN2 IN ('OPEN', 'CLOSED', 'CANCELLED') AND DATALENGTH([status]) = LEN([status])),
    CONSTRAINT [CK_EntryTestSession_Time] CHECK ([start_at] < [end_at] AND DATEPART(second, [start_at]) = 0 AND DATEPART(millisecond, [start_at]) = 0 AND DATEPART(second, [end_at]) = 0 AND DATEPART(millisecond, [end_at]) = 0 AND CAST(DATEADD(minute, 420, [start_at]) AS date) = CAST(DATEADD(minute, 420, [end_at]) AS date)),
    CONSTRAINT [FK_EntryTestSession_Account_cancelled_by] FOREIGN KEY ([cancelled_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_EntryTestSession_Account_grader_id] FOREIGN KEY ([grader_id]) REFERENCES [dbo].[Account] ([account_id])
);

CREATE OR ALTER FUNCTION dbo.ufn_EffectiveEntryTestState
(
    @status varchar(16),
    @startAt datetime2(3),
    @asOfUtc datetime2(3)
)
RETURNS varchar(16)
WITH SCHEMABINDING
AS
BEGIN
    RETURN (
        CASE
            WHEN @status = 'CANCELLED' THEN 'CANCELLED'
            WHEN @asOfUtc >= @startAt THEN 'CLOSED'
            ELSE @status
        END
    );
END;
