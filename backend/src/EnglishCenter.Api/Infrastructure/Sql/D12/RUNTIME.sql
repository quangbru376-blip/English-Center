IF DATABASE_PRINCIPAL_ID(N'ec_app_runtime') IS NOT NULL
BEGIN
    THROW 51212, N'Database role ec_app_runtime already exists.', 1;
END;

CREATE ROLE [ec_app_runtime] AUTHORIZATION [dbo];

-- Tables: SELECT only (2)
GRANT SELECT ON [dbo].[Role] TO [ec_app_runtime];
GRANT SELECT ON [dbo].[Band] TO [ec_app_runtime];

-- Tables: SELECT, INSERT, UPDATE (17)
GRANT SELECT, INSERT, UPDATE ON [dbo].[Account] TO [ec_app_runtime];
GRANT SELECT, INSERT, UPDATE ON [dbo].[StudentProfile] TO [ec_app_runtime];
GRANT SELECT, INSERT, UPDATE ON [dbo].[AuthSession] TO [ec_app_runtime];
GRANT SELECT, INSERT, UPDATE ON [dbo].[Course] TO [ec_app_runtime];
GRANT SELECT, INSERT, UPDATE ON [dbo].[TrainingProgram] TO [ec_app_runtime];
GRANT SELECT, INSERT, UPDATE ON [dbo].[Registration] TO [ec_app_runtime];
GRANT SELECT, INSERT, UPDATE ON [dbo].[EntryTestSession] TO [ec_app_runtime];
GRANT SELECT, INSERT, UPDATE ON [dbo].[EntryTestAttempt] TO [ec_app_runtime];
GRANT SELECT, INSERT, UPDATE ON [dbo].[EntryTestBooking] TO [ec_app_runtime];
GRANT SELECT, INSERT, UPDATE ON [dbo].[BandAssignment] TO [ec_app_runtime];
GRANT SELECT, INSERT, UPDATE ON [dbo].[Class] TO [ec_app_runtime];
GRANT SELECT, INSERT, UPDATE ON [dbo].[Schedule] TO [ec_app_runtime];
GRANT SELECT, INSERT, UPDATE ON [dbo].[Attendance] TO [ec_app_runtime];
GRANT SELECT, INSERT, UPDATE ON [dbo].[Score] TO [ec_app_runtime];
GRANT SELECT, INSERT, UPDATE ON [dbo].[GradeReport] TO [ec_app_runtime];
GRANT SELECT, INSERT, UPDATE ON [dbo].[Invoice] TO [ec_app_runtime];
GRANT SELECT, INSERT, UPDATE ON [dbo].[Payment] TO [ec_app_runtime];

-- Tables: SELECT, INSERT, UPDATE, DELETE (1)
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[ScoreComponentConfig] TO [ec_app_runtime];

-- Tables: SELECT, INSERT only (3)
GRANT SELECT, INSERT ON [dbo].[StudentClassAssignment] TO [ec_app_runtime];
GRANT SELECT, INSERT ON [dbo].[AuditLog] TO [ec_app_runtime];
GRANT SELECT, INSERT ON [dbo].[ReplayOperation] TO [ec_app_runtime];

-- Stored Procedures: EXECUTE (7)
GRANT EXECUTE ON [dbo].[usp_SearchAdmissions] TO [ec_app_runtime];
GRANT EXECUTE ON [dbo].[usp_GetEntryTestRoster] TO [ec_app_runtime];
GRANT EXECUTE ON [dbo].[usp_GetClassGradeReview] TO [ec_app_runtime];
GRANT EXECUTE ON [dbo].[usp_GetFinancialReport] TO [ec_app_runtime];
GRANT EXECUTE ON [dbo].[usp_GetAdmissionsReport] TO [ec_app_runtime];
GRANT EXECUTE ON [dbo].[usp_GetTrainingReport] TO [ec_app_runtime];
GRANT EXECUTE ON [dbo].[usp_AllocateInvoiceNumber] TO [ec_app_runtime];

-- Scalar Function: EXECUTE (1)
GRANT EXECUTE ON [dbo].[ufn_EffectiveEntryTestState] TO [ec_app_runtime];

-- Table-Valued Functions: SELECT (5)
GRANT SELECT ON [dbo].[ufn_BandForScore] TO [ec_app_runtime];
GRANT SELECT ON [dbo].[ufn_RegistrationUnfinished] TO [ec_app_runtime];
GRANT SELECT ON [dbo].[ufn_ClassObligationProgress] TO [ec_app_runtime];
GRANT SELECT ON [dbo].[ufn_GradeTotal] TO [ec_app_runtime];
GRANT SELECT ON [dbo].[ufn_InvoiceDebtFlags] TO [ec_app_runtime];

-- Views: SELECT (6)
GRANT SELECT ON [dbo].[vw_AdmissionsRegistrationFacts] TO [ec_app_runtime];
GRANT SELECT ON [dbo].[vw_EntryTestBookingFacts] TO [ec_app_runtime];
GRANT SELECT ON [dbo].[vw_CurrentScheduleOccurrences] TO [ec_app_runtime];
GRANT SELECT ON [dbo].[vw_TrainingClassFacts] TO [ec_app_runtime];
GRANT SELECT ON [dbo].[vw_FinalizedGradeFacts] TO [ec_app_runtime];
GRANT SELECT ON [dbo].[vw_FinanceInvoiceFacts] TO [ec_app_runtime];
