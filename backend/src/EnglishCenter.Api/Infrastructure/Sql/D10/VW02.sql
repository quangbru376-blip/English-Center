CREATE OR ALTER VIEW [dbo].[vw_EntryTestBookingFacts]
AS
SELECT
    b.[booking_id] AS [bookingId],
    b.[attempt_id] AS [attemptId],
    b.[session_id] AS [sessionId],
    s.[grader_id] AS [graderId],
    s.[status] AS [sessionStatus],
    s.[start_at] AS [sessionStartAt],
    a.[attempt_number] AS [attemptNumber],
    r.[registration_id] AS [registrationId],
    r.[student_id] AS [studentId],
    r.[course_id] AS [courseId],
    b.[booked_at] AS [bookedAt],
    b.[status] AS [bookingStatus],
    r.[status] AS [registrationStatus],
    acc.[full_name] AS [fullName],
    acc.[email] AS [email],
    acc.[phone] AS [phone],
    c.[course_code] AS [courseCode],
    c.[course_name] AS [courseName],
    c.[status] AS [courseStatus],
    CASE
        WHEN b.[status] = 'COMPLETED' AND a.[completed_booking_id] = b.[booking_id]
            THEN a.[score]
        ELSE CAST(NULL AS decimal(5, 2))
    END AS [targetScore],
    CASE
        WHEN b.[status] = 'COMPLETED' AND a.[completed_booking_id] = b.[booking_id]
            THEN a.[graded_at]
        ELSE CAST(NULL AS datetime2(3))
    END AS [targetGradedAt],
    ba.[band_id] AS [bandId],
    bd.[band_code] AS [bandCode],
    bd.[min_score] AS [bandMinScore],
    inv.[status] AS [invoiceStatus]
FROM [dbo].[EntryTestBooking] AS b
INNER JOIN [dbo].[EntryTestAttempt] AS a
    ON a.[attempt_id] = b.[attempt_id]
INNER JOIN [dbo].[EntryTestSession] AS s
    ON s.[session_id] = b.[session_id]
INNER JOIN [dbo].[Registration] AS r
    ON r.[registration_id] = a.[registration_id]
INNER JOIN [dbo].[StudentProfile] AS sp
    ON sp.[student_id] = r.[student_id]
INNER JOIN [dbo].[Account] AS acc
    ON acc.[account_id] = sp.[account_id]
INNER JOIN [dbo].[Course] AS c
    ON c.[course_id] = r.[course_id]
LEFT JOIN [dbo].[BandAssignment] AS ba
    ON ba.[registration_id] = r.[registration_id]
LEFT JOIN [dbo].[Band] AS bd
    ON bd.[band_id] = ba.[band_id]
LEFT JOIN [dbo].[Invoice] AS inv
    ON inv.[registration_id] = r.[registration_id];
