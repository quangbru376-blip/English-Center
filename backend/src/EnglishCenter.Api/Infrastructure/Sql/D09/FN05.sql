CREATE OR ALTER FUNCTION dbo.ufn_GradeTotal
(
    @classAssignmentId int
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        CAST(agg.requiredComponentCount AS int) AS requiredComponentCount,
        CAST(agg.enteredComponentCount AS int) AS enteredComponentCount,
        CAST(CASE
            WHEN agg.requiredComponentCount > 0 AND agg.enteredComponentCount = agg.requiredComponentCount THEN 1
            ELSE 0
        END AS bit) AS isComplete,
        CASE
            WHEN agg.requiredComponentCount > 0 AND agg.enteredComponentCount = agg.requiredComponentCount
                THEN CAST(ROUND(agg.weightedSum / 100.0, 2) AS decimal(5,2))
            ELSE CAST(NULL AS decimal(5,2))
        END AS total
    FROM dbo.StudentClassAssignment sca
    INNER JOIN dbo.Class c ON c.class_id = sca.class_id
    CROSS APPLY
    (
        SELECT
            COUNT(DISTINCT scc.component_id) AS requiredComponentCount,
            COUNT(DISTINCT s.component_id) AS enteredComponentCount,
            SUM(s.score_value * scc.weight) AS weightedSum
        FROM dbo.ScoreComponentConfig scc
        LEFT JOIN dbo.Score s ON s.class_assignment_id = sca.class_assignment_id
                             AND s.component_id = scc.component_id
        WHERE scc.program_id = c.program_id
    ) agg
    WHERE sca.class_assignment_id = @classAssignmentId
);
