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
