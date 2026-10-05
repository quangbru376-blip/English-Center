# D10 / VW01 — vw_AdmissionsRegistrationFacts

Read COMPLETE this card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md before writing. Read ONLY these three paths and exact output if it exists. Do not follow links/read AGENTS/memory/other source. No commands/search/scans/network/SQL/Git/MCP/browser/subagents/secrets. Only view_file/write_to_file/replace_file_content. Codex independently tests/reviews/applies. SQL Server160, exact decimal/NULL/time/full-set accepted E03/E04 overrides stale SQLite/API/seed guidance. No packages, grants, migration files, GO/USE, GETDATE, audit, or mutation. One CREATE OR ALTER VIEW, explicit projection, no SELECT *. CamelCase view aliases specified below are manager integration choices, not web rights.

Exact output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D10/VW01.sql

One row/Registration. Inner join its StudentProfile/Account/Course; left join unique BandAssignment/Band, Invoice, Assignment/Class. Exact aliases: registrationId,studentId,fullName,email,phone,courseId,courseCode,courseName,courseStatus,registrationStatus,createdAt,admissionConfirmedAt,admissionApprovedAt,bandId,bandCode,bandMinScore,invoiceId,invoiceNumber,invoiceAmount,invoiceStatus,classId,assignmentId,classCode,classStatus,registrationVersion. Source current Account full_name/email/phone; registration confirmation/approval timestamps; version its row_version. No DOB/address/password/profile snapshot. All nullable joined bundles reflect real absence; do not coalesce. Include every registration status/current course status. Unique joins preserve grain.

Exact initial schema:

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

CREATE TABLE [dbo].[StudentProfile] (
    [student_id] int NOT NULL IDENTITY,
    [account_id] int NOT NULL,
    [date_of_birth] date NOT NULL,
    [address] nvarchar(510) NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_StudentProfile] PRIMARY KEY ([student_id]),
    CONSTRAINT [FK_StudentProfile_Account_account_id] FOREIGN KEY ([account_id]) REFERENCES [dbo].[Account] ([account_id])
);

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

CREATE TABLE [dbo].[BandAssignment] (
    [band_assignment_id] int NOT NULL IDENTITY,
    [registration_id] int NOT NULL,
    [best_attempt_id] int NOT NULL,
    [band_id] int NOT NULL,
    [assigned_at] datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME()),
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_BandAssignment] PRIMARY KEY ([band_assignment_id]),
    CONSTRAINT [FK_BandAssignment_Band_band_id] FOREIGN KEY ([band_id]) REFERENCES [dbo].[Band] ([band_id]),
    CONSTRAINT [FK_BandAssignment_Registration_registration_id] FOREIGN KEY ([registration_id]) REFERENCES [dbo].[Registration] ([registration_id])
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

CREATE TABLE [dbo].[Invoice] (
    [invoice_id] int NOT NULL IDENTITY,
    [invoice_number] varchar(16) NOT NULL,
    [registration_id] int NOT NULL,
    [amount] decimal(18,0) NOT NULL,
    [issued_at] datetime2(3) NOT NULL,
    [due_date] date NOT NULL,
    [status] varchar(16) NOT NULL DEFAULT 'UNPAID',
    [cancelled_by] int NULL,
    [cancelled_at] datetime2(3) NULL,
    [cancellation_reason] nvarchar(2000) NULL,
    [cancelled_from_status] varchar(16) NULL,
    [cancellation_operation_id] uniqueidentifier NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_Invoice] PRIMARY KEY ([invoice_id]),
    CONSTRAINT [CK_Invoice_Amount] CHECK ([amount] > 0),
    CONSTRAINT [CK_Invoice_CancelledBundle] CHECK (([status] = 'CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancelled_from_status] IS NOT NULL AND [cancellation_operation_id] IS NOT NULL AND [cancelled_from_status] COLLATE Latin1_General_100_BIN2 = 'UNPAID' AND DATALENGTH([cancelled_from_status]) = LEN([cancelled_from_status])) OR ([status] <> 'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancelled_from_status] IS NULL AND [cancellation_operation_id] IS NULL)),
    CONSTRAINT [CK_Invoice_NumberPattern] CHECK (DATALENGTH([invoice_number]) = 16 AND DATALENGTH([invoice_number]) = LEN([invoice_number]) AND [invoice_number] COLLATE Latin1_General_100_BIN2 LIKE 'HD-[0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'),
    CONSTRAINT [CK_Invoice_Status] CHECK ([status] COLLATE Latin1_General_100_BIN2 IN ('UNPAID', 'PAID', 'CANCELLED') AND DATALENGTH([status]) = LEN([status])),
    CONSTRAINT [FK_Invoice_Account_cancelled_by] FOREIGN KEY ([cancelled_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Invoice_Registration_registration_id] FOREIGN KEY ([registration_id]) REFERENCES [dbo].[Registration] ([registration_id])
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