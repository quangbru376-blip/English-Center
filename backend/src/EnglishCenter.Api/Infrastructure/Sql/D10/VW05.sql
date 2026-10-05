CREATE OR ALTER VIEW [dbo].[vw_FinalizedGradeFacts]
AS
SELECT
    report.[report_id] AS [reportId],
    report.[class_assignment_id] AS [classAssignmentId],
    assignment.[registration_id] AS [registrationId],
    registration.[student_id] AS [studentId],
    assignment.[class_id] AS [classId],
    program.[course_id] AS [courseId],
    program.[band_id] AS [bandId],
    class.[teacher_id] AS [teacherId],
    report.[final_score] AS [finalScore],
    CAST(CASE WHEN report.[final_score] >= 50 THEN 1 ELSE 0 END AS bit) AS [isPassing],
    report.[finalized_at] AS [finalizedAt]
FROM [dbo].[GradeReport] AS report
INNER JOIN [dbo].[StudentClassAssignment] AS assignment
    ON assignment.[class_assignment_id] = report.[class_assignment_id]
INNER JOIN [dbo].[Registration] AS registration
    ON registration.[registration_id] = assignment.[registration_id]
INNER JOIN [dbo].[Class] AS class
    ON class.[class_id] = assignment.[class_id]
INNER JOIN [dbo].[TrainingProgram] AS program
    ON program.[program_id] = class.[program_id]
WHERE report.[status] = 'FINALIZED';
