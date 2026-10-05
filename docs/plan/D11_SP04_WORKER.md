# D11 / SP04 — usp_GetFinancialReport

Read COMPLETE this card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md first. Read ONLY these3paths and exact output if it exists. Do not follow links/read AGENTS/other source/memory. No commands/search/scans/network/SQL/Git/MCP/browser/subagents/secrets. Only view_file/write_to_file/replace_file_content. Codex independently tests/reviews/applies. SQLServer160/exact decimal/time/NULL/full-set/current role/privacy E03/E04 override stale SQLite/API/seed rules. No packages/grants/GO/USE/migration/GETDATE or security/profile outputs. One CREATE OR ALTER PROCEDURE; SET NOCOUNT ON; TRY...CATCH THROW. Explicit projections, typed NULL, bigint counts and checked bigint offset=(CONVERT(bigint,@page)-1)*@pageSize. Values parameters only, literal contains LIKE escape '~' then '%' '_' '[' (REPLACE nested), no user SQL/sort concatenation. Null pair bounds follow canonical. No business writes except SP07 counter. No silent truncation, fake0, guessed seed, audit or replay.

Manager SQL integration choices: SP01–06 REQUIRE existing caller transaction with isolation_level=5 (SNAPSHOT) and XACT_STATE()=1. If absent/wrong isolation THROW51104. Never begin/commit/rollback caller transaction. This preserves service-owned transaction plus same-snapshot parent EF reads/options. Read SPs may use table variables/temp staging only, no business DML. Error51101 unauthorized active role/restricted account;51102 scope miss (parent absent/Teacher not assigned/no visible grade report) maps404 in future service;51103 invalid filters/page/date/sort; SP07 absent transaction51107/year/overflow51108. TRY/CATCH THROW without own rollback. Current account active AND must_change_password=0 for role defenses; role_code exact accepted case. SP04–06 no actor parameter or web auth claim (service future).

Exact output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D11/SP04.sql

Accepted exact signature/result contract:
dbo.usp_GetFinancialReport → API-150/151/154

Parameters fromUtc/toExclusiveUtc datetime2(3) N, courseId int Y,
overdueOnly bit N, todayVn date N, page/pageSize int Y.
Both pagination ints N for API150/151; **both null** for CSV all filtered results.
No actor/student/invoice selector. Report projection non-identifying aggregate
for either caller; web role/export checked Service. Report readers SQL roles
get these aggregates only, no method/reference/source IDs.

RS1 one row: totalCount bigint N, revenueAmount/billedAmount/currentDebtAmount/
overdueDebtAmount varchar(38) N. RS2 one Course group row:
courseId int N, courseCode varchar(30) N, courseName nvarchar(300) N,
four amount columns same varchar(38) N; order courseCode ASC/courseId ASC.
RS3 Course options: courseId int N, courseCode varchar(30) N,
courseName nvarchar(300) N, status E03(T05.status) N; scope D10 incl history.
Amounts exact decimal38 converted in SQL to digit strings after aggregate;
no scientific spelling. UNION cohort Course keys, totals independent pagination.
CSV Service buffers bounded encoded file per D10 before response headers,
not SP row cap/partial aggregation. QueriedAt/today/appliedFilters server context,
not SQL historical-as-of parameters. Empty RS1 four"0", RS2 empty, RS3 options.


Manager integration requirements:
from<to; both page ints null CSV OR page>=1,size1..100 (reject one-null). course positive/null; overdue bit/today nonnull. Revenue uses paidAmount at paidAt in halfopenperiod (do not replace by Invoice amount). Billed invoice amount UNPAID/PAID issuedAt period, excludesCANCELLED. Debt allcurrentUNPAID independentdate; overdueOnly affects ONLY debt subset via FN06 with passedtoday. Aggregate separately/CASE independently before uniongroups; Course group exists iff some qualifying FACT even amount0 absent aggregates (allmoneypositive). SUM(CONVERT(decimal(38,0),amount)) BEFORE varchar(38) conversion; totals whole unionnotpage. Groups perCourse ordercode/id, no invoice/payment/student IDs in any output. Courseoptions full referenced/history plus ACTIVE per shared scope below. No filter dates sharedbase or billed-revenue debt. Empty totals4string0/groupempty/options retained.
Full report Course options (accepted DTO31.1): ACTIVE Courses plus INACTIVE Courses referenced by retained Registration or Class/Program/Invoice facts, independent current filters/page. Do not return unreferenced inactive Course. Order code/id. Invoice/Registration factcoursecoversallregistrations; Class/Program adds referencedcourse. ExactAlias courseId,courseCode,courseName,status.


Referenced view projection contract (views generated separately):
backend/src/EnglishCenter.Api/Infrastructure/Sql/D10/VW06.sql

One Invoice row plus max one Payment via unique invoice_id. Join Registration/Course, left Payment. Aliases invoiceId,registrationId,studentId,courseId,courseCode,courseName,courseStatus,invoiceNumber,amount,invoiceStatus,issuedAt,dueDate,paymentId,paidAt,paidAmount. amount decimal18,0, paidAt Payment.paid_at; paidAmount Payment.amount_paid (revenue must use actual Payment amount, not Invoice amount). Include all states/history. No debt=billed-revenue, no GETDATE/FN06 without supplied today (view cannot accept time). No payment method/reference/contact/profile columns: reporting allowed SELECT VW06. Reference IDs are explicitly accepted E03 finance facts, Director later SP-only.

Exact initial schema:

CREATE TABLE [dbo].[Course] (
    [course_id] int NOT NULL IDENTITY,
    [course_code] varchar(30) NOT NULL,
    [course_name] nvarchar(300) NOT NULL,
    [base_price] decimal(18,0) NOT NULL,
    [status] varchar(16) NOT NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_Course] PRIMARY KEY ([course_id]),
    CONSTRAINT [CK_Course_Code] CHECK (DATALENGTH([course_code]) BETWEEN 1 AND 30 AND DATALENGTH([course_code]) = LEN([course_code]) AND [course_code] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z0-9_-]%'),
    CONSTRAINT [CK_Course_Price] CHECK ([base_price] > 0),
    CONSTRAINT [CK_Course_Status] CHECK ([status] COLLATE Latin1_General_100_BIN2 IN ('ACTIVE','INACTIVE') AND DATALENGTH([status]) = LEN([status]))
);

CREATE TABLE [dbo].[Registration] (
    [registration_id] int NOT NULL IDENTITY,
    [student_id] int NOT NULL,
    [course_id] int NOT NULL,
    [status] varchar(16) NOT NULL DEFAULT 'PENDING',
    [is_unfinished] bit NOT NULL DEFAULT CAST(1 AS bit),
    [created_at] datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME()),
    [admission_confirmed_at] datetime2(3) NULL,
    [admission_approved_by] int NULL,
    [admission_approved_at] datetime2(3) NULL,
    [rejected_by] int NULL,
    [rejected_at] datetime2(3) NULL,
    [rejection_reason] nvarchar(2000) NULL,
    [rejected_from_status] varchar(16) NULL,
    [rejection_operation_id] uniqueidentifier NULL,
    [cancelled_by] int NULL,
    [cancelled_at] datetime2(3) NULL,
    [cancellation_reason] nvarchar(2000) NULL,
    [cancelled_from_status] varchar(16) NULL,
    [cancellation_operation_id] uniqueidentifier NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_Registration] PRIMARY KEY ([registration_id]),
    CONSTRAINT [CK_Registration_ApprovedBundle] CHECK (([admission_approved_by] IS NULL AND [admission_approved_at] IS NULL) OR ([admission_approved_by] IS NOT NULL AND [admission_approved_at] IS NOT NULL)),
    CONSTRAINT [CK_Registration_CancelledBundle] CHECK (([status] = 'CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancelled_from_status] IS NOT NULL AND [cancelled_from_status] COLLATE Latin1_General_100_BIN2 IN ('PENDING', 'TESTING', 'BAND_ASSIGNED', 'CONFIRMED', 'ADMITTED') AND DATALENGTH([cancelled_from_status]) = LEN([cancelled_from_status]) AND [cancellation_operation_id] IS NOT NULL) OR ([status] <> 'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancelled_from_status] IS NULL AND [cancellation_operation_id] IS NULL)),
    CONSTRAINT [CK_Registration_RejectedBundle] CHECK (([status] = 'REJECTED' AND [rejected_by] IS NOT NULL AND [rejected_at] IS NOT NULL AND [rejection_reason] IS NOT NULL AND LEN([rejection_reason]) > 0 AND [rejected_from_status] IS NOT NULL AND [rejected_from_status] COLLATE Latin1_General_100_BIN2 = 'CONFIRMED' AND DATALENGTH([rejected_from_status]) = LEN([rejected_from_status]) AND [rejection_operation_id] IS NOT NULL) OR ([status] <> 'REJECTED' AND [rejected_by] IS NULL AND [rejected_at] IS NULL AND [rejection_reason] IS NULL AND [rejected_from_status] IS NULL AND [rejection_operation_id] IS NULL)),
    CONSTRAINT [CK_Registration_Status] CHECK ([status] COLLATE Latin1_General_100_BIN2 IN ('PENDING', 'TESTING', 'BAND_ASSIGNED', 'CONFIRMED', 'ADMITTED', 'REJECTED', 'CANCELLED') AND DATALENGTH([status]) = LEN([status])),
    CONSTRAINT [FK_Registration_Account_admission_approved_by] FOREIGN KEY ([admission_approved_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Registration_Account_cancelled_by] FOREIGN KEY ([cancelled_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Registration_Account_rejected_by] FOREIGN KEY ([rejected_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Registration_Course_course_id] FOREIGN KEY ([course_id]) REFERENCES [dbo].[Course] ([course_id]),
    CONSTRAINT [FK_Registration_StudentProfile_student_id] FOREIGN KEY ([student_id]) REFERENCES [dbo].[StudentProfile] ([student_id])
);

CREATE TABLE [dbo].[Class] (
    [class_id] int NOT NULL IDENTITY,
    [program_id] int NOT NULL,
    [teacher_id] int NOT NULL,
    [class_code] varchar(30) NOT NULL,
    [status] varchar(16) NOT NULL DEFAULT 'PLANNED',
    [max_size] int NOT NULL,
    [created_at] datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME()),
    [cancelled_by] int NULL,
    [cancelled_at] datetime2(3) NULL,
    [cancellation_reason] nvarchar(2000) NULL,
    [cancellation_operation_id] uniqueidentifier NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_Class] PRIMARY KEY ([class_id]),
    CONSTRAINT [CK_Class_CancellationBundle] CHECK (([status] = 'CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancellation_reason] IS NOT NULL AND LEN([cancellation_reason]) > 0 AND [cancellation_operation_id] IS NOT NULL) OR ([status] <> 'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancellation_operation_id] IS NULL)),
    CONSTRAINT [CK_Class_Code] CHECK (DATALENGTH([class_code]) BETWEEN 1 AND 30 AND DATALENGTH([class_code]) = LEN([class_code]) AND [class_code] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z0-9_-]%'),
    CONSTRAINT [CK_Class_MaxSize] CHECK ([max_size] BETWEEN 1 AND 15),
    CONSTRAINT [CK_Class_Status] CHECK ([status] COLLATE Latin1_General_100_BIN2 IN ('PLANNED', 'OPEN', 'ONGOING', 'COMPLETED', 'CANCELLED') AND DATALENGTH([status]) = LEN([status])),
    CONSTRAINT [FK_Class_Account_cancelled_by] FOREIGN KEY ([cancelled_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Class_Account_teacher_id] FOREIGN KEY ([teacher_id]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Class_TrainingProgram_program_id] FOREIGN KEY ([program_id]) REFERENCES [dbo].[TrainingProgram] ([program_id])
);

CREATE TABLE [dbo].[TrainingProgram] (
    [program_id] int NOT NULL IDENTITY,
    [course_id] int NOT NULL,
    [band_id] int NOT NULL,
    [program_name] nvarchar(300) NOT NULL,
    [status] varchar(16) NOT NULL DEFAULT 'DRAFT',
    [expected_lesson_count] int NOT NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_TrainingProgram] PRIMARY KEY ([program_id]),
    CONSTRAINT [CK_TrainingProgram_ExpectedLessons] CHECK ([expected_lesson_count] > 0),
    CONSTRAINT [CK_TrainingProgram_Status] CHECK ([status] COLLATE Latin1_General_100_BIN2 IN ('DRAFT','ACTIVE','INACTIVE') AND DATALENGTH([status]) = LEN([status])),
    CONSTRAINT [FK_TrainingProgram_Band_band_id] FOREIGN KEY ([band_id]) REFERENCES [dbo].[Band] ([band_id]),
    CONSTRAINT [FK_TrainingProgram_Course_course_id] FOREIGN KEY ([course_id]) REFERENCES [dbo].[Course] ([course_id])
);

CREATE OR ALTER FUNCTION dbo.ufn_InvoiceDebtFlags
(
    @status varchar(16),
    @dueDate date,
    @todayVn date
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        CAST(CASE
            WHEN @status = 'UNPAID' AND @dueDate < @todayVn THEN 1
            ELSE 0
        END AS bit) AS isOverdue,
        CAST(CASE
            WHEN @status = 'UNPAID' AND @dueDate < @todayVn THEN DATEDIFF(day, @dueDate, @todayVn)
            ELSE 0
        END AS int) AS daysOverdue
);
