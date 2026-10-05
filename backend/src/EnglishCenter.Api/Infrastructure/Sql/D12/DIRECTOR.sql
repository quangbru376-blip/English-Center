-- D12: Role and permissions for Director reporting (ec_director_reporting)

IF DATABASE_PRINCIPAL_ID('ec_director_reporting') IS NOT NULL
BEGIN
    THROW 51212, 'Role ec_director_reporting already exists.', 1;
END;

CREATE ROLE [ec_director_reporting] AUTHORIZATION [dbo];

GRANT EXECUTE ON [dbo].[usp_GetFinancialReport] TO [ec_director_reporting];
GRANT EXECUTE ON [dbo].[usp_GetAdmissionsReport] TO [ec_director_reporting];
GRANT EXECUTE ON [dbo].[usp_GetTrainingReport] TO [ec_director_reporting];

DENY SELECT ON [dbo].[Account] TO [ec_director_reporting];
DENY SELECT ON [dbo].[StudentProfile] TO [ec_director_reporting];
DENY SELECT ON [dbo].[Score] TO [ec_director_reporting];
DENY SELECT ON [dbo].[GradeReport] TO [ec_director_reporting];
