CREATE OR ALTER VIEW [dbo].[vw_TrainingClassFacts]
AS
SELECT
    c.[class_id] AS [classId],
    c.[program_id] AS [programId],
    c.[teacher_id] AS [teacherId],
    c.[class_code] AS [classCode],
    co.[course_id] AS [courseId],
    co.[course_code] AS [courseCode],
    co.[course_name] AS [courseName],
    co.[status] AS [courseStatus],
    b.[band_id] AS [bandId],
    b.[band_code] AS [bandCode],
    b.[min_score] AS [bandMinScore],
    c.[status] AS [classStatus],
    c.[max_size] AS [maxSize],
    CAST(ISNULL(a.[placed_count], 0) AS int) AS [placedCount],
    p.[expectedLessonCount] AS [expectedCount],
    p.[currentLessonCount] AS [currentCount],
    p.[heldLessonCount] AS [heldCount],
    p.[currentFinalExamCount] AS [examCurrentCount],
    p.[heldFinalExamCount] AS [examHeldCount]
FROM [dbo].[Class] AS c
INNER JOIN [dbo].[TrainingProgram] AS tp
    ON tp.[program_id] = c.[program_id]
INNER JOIN [dbo].[Course] AS co
    ON co.[course_id] = tp.[course_id]
INNER JOIN [dbo].[Band] AS b
    ON b.[band_id] = tp.[band_id]
CROSS APPLY [dbo].[ufn_ClassObligationProgress](c.[class_id]) AS p
LEFT JOIN
(
    SELECT
        sca.[class_id],
        COUNT(sca.[class_assignment_id]) AS [placed_count]
    FROM [dbo].[StudentClassAssignment] AS sca
    GROUP BY sca.[class_id]
) AS a
    ON a.[class_id] = c.[class_id];
