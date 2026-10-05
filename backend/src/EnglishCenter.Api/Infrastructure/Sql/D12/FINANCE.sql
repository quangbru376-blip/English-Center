-- D12 FINANCE: ec_finance_reporting role and object-level permissions
-- Fail-fast guard: ensure role does not already exist
IF DATABASE_PRINCIPAL_ID('ec_finance_reporting') IS NOT NULL
BEGIN
    THROW 51212, 'Role ec_finance_reporting already exists.', 1;
END;

CREATE ROLE [ec_finance_reporting] AUTHORIZATION [dbo];

-- Explicit dbo object-level grants
GRANT SELECT ON [dbo].[vw_FinanceInvoiceFacts] TO [ec_finance_reporting];
GRANT EXECUTE ON [dbo].[usp_GetFinancialReport] TO [ec_finance_reporting];

-- Explicit dbo object-level denys
DENY SELECT ON [dbo].[Account] TO [ec_finance_reporting];
DENY SELECT ON [dbo].[StudentProfile] TO [ec_finance_reporting];
DENY SELECT ON [dbo].[Score] TO [ec_finance_reporting];
DENY SELECT ON [dbo].[GradeReport] TO [ec_finance_reporting];
