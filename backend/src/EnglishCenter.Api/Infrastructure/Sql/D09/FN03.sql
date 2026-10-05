CREATE OR ALTER FUNCTION [dbo].[ufn_RegistrationUnfinished]
(
    @registrationId INT
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        CAST(
            CASE
                WHEN [r].[status] IN ('CANCELLED', 'REJECTED') THEN 0
                WHEN EXISTS
                (
                    SELECT 1
                    FROM [dbo].[StudentClassAssignment] AS [sca]
                    INNER JOIN [dbo].[Class] AS [c]
                        ON [c].[class_id] = [sca].[class_id]
                    INNER JOIN [dbo].[GradeReport] AS [gr]
                        ON [gr].[class_assignment_id] = [sca].[class_assignment_id]
                    WHERE [sca].[registration_id] = [r].[registration_id]
                      AND [c].[status] = 'COMPLETED'
                      AND [gr].[status] = 'FINALIZED'
                ) THEN 0
                ELSE 1
            END AS BIT
        ) AS [isUnfinished]
    FROM [dbo].[Registration] AS [r]
    WHERE [r].[registration_id] = @registrationId
);
