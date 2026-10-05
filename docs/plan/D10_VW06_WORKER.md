# D10 / VW06 — vw_FinanceInvoiceFacts

Read COMPLETE this card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md before writing. Read ONLY these three paths and exact output if it exists. Do not follow links/read AGENTS/memory/other source. No commands/search/scans/network/SQL/Git/MCP/browser/subagents/secrets. Only view_file/write_to_file/replace_file_content. Codex independently tests/reviews/applies. SQL Server160, exact decimal/NULL/time/full-set accepted E03/E04 overrides stale SQLite/API/seed guidance. No packages, grants, migration files, GO/USE, GETDATE, audit, or mutation. One CREATE OR ALTER VIEW, explicit projection, no SELECT *. CamelCase view aliases specified below are manager integration choices, not web rights.

Exact output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D10/VW06.sql

One Invoice row plus max one Payment via unique invoice_id. Join Registration/Course, left Payment. Aliases invoiceId,registrationId,studentId,courseId,courseCode,courseName,courseStatus,invoiceNumber,amount,invoiceStatus,issuedAt,dueDate,paymentId,paidAt,paidAmount. amount decimal18,0, paidAt Payment.paid_at; paidAmount Payment.amount_paid (revenue must use actual Payment amount, not Invoice amount). Include all states/history. No debt=billed-revenue, no GETDATE/FN06 without supplied today (view cannot accept time). No payment method/reference/contact/profile columns: reporting allowed SELECT VW06. Reference IDs are explicitly accepted E03 finance facts, Director later SP-only.

Exact initial schema:

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

CREATE TABLE [dbo].[Payment] (
    [payment_id] int NOT NULL IDENTITY,
    [invoice_id] int NOT NULL,
    [amount_paid] decimal(18,0) NOT NULL,
    [method] varchar(16) NOT NULL,
    [transaction_reference] nvarchar(200) NULL,
    [confirmed_by] int NOT NULL,
    [paid_at] datetime2(3) NOT NULL,
    [last_corrected_by] int NULL,
    [last_corrected_at] datetime2(3) NULL,
    [last_correction_reason] nvarchar(2000) NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_Payment] PRIMARY KEY ([payment_id]),
    CONSTRAINT [CK_Payment_Amount] CHECK ([amount_paid] > 0),
    CONSTRAINT [CK_Payment_CorrectionBundle] CHECK (([last_corrected_by] IS NULL AND [last_corrected_at] IS NULL AND [last_correction_reason] IS NULL) OR ([last_corrected_by] IS NOT NULL AND [last_corrected_at] IS NOT NULL AND [last_correction_reason] IS NOT NULL AND LEN([last_correction_reason]) > 0)),
    CONSTRAINT [CK_Payment_Method] CHECK ([method] COLLATE Latin1_General_100_BIN2 IN ('CASH', 'TRANSFER') AND DATALENGTH([method]) = LEN([method])),
    CONSTRAINT [CK_Payment_Reference] CHECK ([method] <> 'TRANSFER' OR ([transaction_reference] IS NOT NULL AND LEN([transaction_reference]) > 0)),
    CONSTRAINT [FK_Payment_Account_confirmed_by] FOREIGN KEY ([confirmed_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Payment_Account_last_corrected_by] FOREIGN KEY ([last_corrected_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Payment_Invoice_invoice_id] FOREIGN KEY ([invoice_id]) REFERENCES [dbo].[Invoice] ([invoice_id])
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