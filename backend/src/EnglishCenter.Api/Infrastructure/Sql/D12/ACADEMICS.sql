-- D12 ACADEMICS: Database role and object-level permissions for ec_academics_reporting

IF DATABASE_PRINCIPAL_ID('ec_academics_reporting') IS NOT NULL
BEGIN
    THROW 51212, 'Role ec_academics_reporting already exists.', 1;
END;

CREATE ROLE [ec_academics_reporting] AUTHORIZATION [dbo];

GRANT SELECT ON [dbo].[vw_CurrentScheduleOccurrences] TO [ec_academics_reporting];
GRANT SELECT ON [dbo].[vw_TrainingClassFacts] TO [ec_academics_reporting];

GRANT EXECUTE ON [dbo].[usp_GetTrainingReport] TO [ec_academics_reporting];

DENY SELECT ON [dbo].[Account] TO [ec_academics_reporting];
DENY SELECT ON [dbo].[StudentProfile] TO [ec_academics_reporting];
DENY SELECT ON [dbo].[Score] TO [ec_academics_reporting];
DENY SELECT ON [dbo].[GradeReport] TO [ec_academics_reporting];
