CREATE OR ALTER VIEW [dbo].[vw_AdmissionsRegistrationFacts]
AS
SELECT
    [r].[registration_id] AS [registrationId],
    [r].[student_id] AS [studentId],
    [a].[full_name] AS [fullName],
    [a].[email] AS [email],
    [a].[phone] AS [phone],
    [c].[course_id] AS [courseId],
    [c].[course_code] AS [courseCode],
    [c].[course_name] AS [courseName],
    [c].[status] AS [courseStatus],
    [r].[status] AS [registrationStatus],
    [r].[created_at] AS [createdAt],
    [r].[admission_confirmed_at] AS [admissionConfirmedAt],
    [r].[admission_approved_at] AS [admissionApprovedAt],
    [b].[band_id] AS [bandId],
    [b].[band_code] AS [bandCode],
    [b].[min_score] AS [bandMinScore],
    [i].[invoice_id] AS [invoiceId],
    [i].[invoice_number] AS [invoiceNumber],
    [i].[amount] AS [invoiceAmount],
    [i].[status] AS [invoiceStatus],
    [cls].[class_id] AS [classId],
    [sca].[class_assignment_id] AS [assignmentId],
    [cls].[class_code] AS [classCode],
    [cls].[status] AS [classStatus],
    [r].[row_version] AS [registrationVersion]
FROM [dbo].[Registration] AS [r]
INNER JOIN [dbo].[StudentProfile] AS [sp]
    ON [sp].[student_id] = [r].[student_id]
INNER JOIN [dbo].[Account] AS [a]
    ON [a].[account_id] = [sp].[account_id]
INNER JOIN [dbo].[Course] AS [c]
    ON [c].[course_id] = [r].[course_id]
LEFT JOIN [dbo].[BandAssignment] AS [ba]
    ON [ba].[registration_id] = [r].[registration_id]
LEFT JOIN [dbo].[Band] AS [b]
    ON [b].[band_id] = [ba].[band_id]
LEFT JOIN [dbo].[Invoice] AS [i]
    ON [i].[registration_id] = [r].[registration_id]
LEFT JOIN [dbo].[StudentClassAssignment] AS [sca]
    ON [sca].[registration_id] = [r].[registration_id]
LEFT JOIN [dbo].[Class] AS [cls]
    ON [cls].[class_id] = [sca].[class_id];
