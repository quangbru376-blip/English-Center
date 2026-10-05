# D10 / VW04 — vw_TrainingClassFacts

Read COMPLETE this card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md before writing. Read ONLY these three paths and exact output if it exists. Do not follow links/read AGENTS/memory/other source. No commands/search/scans/network/SQL/Git/MCP/browser/subagents/secrets. Only view_file/write_to_file/replace_file_content. Codex independently tests/reviews/applies. SQL Server160, exact decimal/NULL/time/full-set accepted E03/E04 overrides stale SQLite/API/seed guidance. No packages, grants, migration files, GO/USE, GETDATE, audit, or mutation. One CREATE OR ALTER VIEW, explicit projection, no SELECT *. CamelCase view aliases specified below are manager integration choices, not web rights.

Exact output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D10/VW04.sql

One row/Class including CANCELLED. Join Program/Course/Band, CROSS APPLY dbo.ufn_ClassObligationProgress(class_id) exact FN04. Count placed assignments separately (aggregate before joining; explicit COUNT and int result) to avoid schedules x assignments multiplication. Aliases classId,programId,teacherId,classCode,courseId,courseCode,courseName,courseStatus,bandId,bandCode,bandMinScore,classStatus,maxSize,placedCount,expectedCount,currentCount,heldCount,examCurrentCount,examHeldCount. expectedCount int=FN expectedLessonCount; currentCount/heldCount bigint; exams int. Preserve raw cancelled counts; SP06 later NULLs cancelled progress. No calculated progress percent here.

Exact initial schema:

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

CREATE TABLE [dbo].[Band] (
    [band_id] int NOT NULL IDENTITY,
    [band_code] varchar(30) NOT NULL,
    [min_score] decimal(5,2) NOT NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_Band] PRIMARY KEY ([band_id]),
    CONSTRAINT [CK_Band_Code] CHECK (DATALENGTH([band_code]) BETWEEN 1 AND 30 AND [band_code] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^ -~]%' AND LEN([band_code]) > 0),
    CONSTRAINT [CK_Band_MinScore] CHECK ([min_score] >= 0 AND [min_score] <= 100)
);

CREATE TABLE [dbo].[StudentClassAssignment] (
    [class_assignment_id] int NOT NULL IDENTITY,
    [class_id] int NOT NULL,
    [registration_id] int NOT NULL,
    [assigned_by] int NOT NULL,
    [assigned_at] datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_StudentClassAssignment] PRIMARY KEY ([class_assignment_id]),
    CONSTRAINT [FK_Assignment_Account_assigned_by] FOREIGN KEY ([assigned_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Assignment_Class_class_id] FOREIGN KEY ([class_id]) REFERENCES [dbo].[Class] ([class_id]),
    CONSTRAINT [FK_Assignment_Registration_registration_id] FOREIGN KEY ([registration_id]) REFERENCES [dbo].[Registration] ([registration_id])
);

CREATE OR ALTER FUNCTION [dbo].[ufn_ClassObligationProgress]
(
    @classId int
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        tp.[expected_lesson_count] AS [expectedLessonCount],
        COUNT_BIG(CASE WHEN s.[session_type] = 'LESSON' THEN 1 END) AS [currentLessonCount],
        COUNT_BIG(CASE WHEN s.[session_type] = 'LESSON' AND s.[occurrence_state] = 'HELD' THEN 1 END) AS [heldLessonCount],
        COUNT(CASE WHEN s.[session_type] = 'FINAL_EXAM' THEN 1 END) AS [currentFinalExamCount],
        COUNT(CASE WHEN s.[session_type] = 'FINAL_EXAM' AND s.[occurrence_state] = 'HELD' THEN 1 END) AS [heldFinalExamCount]
    FROM [dbo].[Class] AS c
    INNER JOIN [dbo].[TrainingProgram] AS tp
        ON tp.[program_id] = c.[program_id]
    LEFT JOIN [dbo].[Schedule] AS s
        ON s.[class_id] = c.[class_id]
        AND s.[is_current] = 1
    WHERE c.[class_id] = @classId
    GROUP BY c.[class_id], tp.[expected_lesson_count]
);
