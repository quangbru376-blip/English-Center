CREATE OR ALTER VIEW [dbo].[vw_FinanceInvoiceFacts]
AS
SELECT
    [i].[invoice_id] AS [invoiceId],
    [i].[registration_id] AS [registrationId],
    [r].[student_id] AS [studentId],
    [c].[course_id] AS [courseId],
    [c].[course_code] AS [courseCode],
    [c].[course_name] AS [courseName],
    [c].[status] AS [courseStatus],
    [i].[invoice_number] AS [invoiceNumber],
    [i].[amount] AS [amount],
    [i].[status] AS [invoiceStatus],
    [i].[issued_at] AS [issuedAt],
    [i].[due_date] AS [dueDate],
    [p].[payment_id] AS [paymentId],
    [p].[paid_at] AS [paidAt],
    [p].[amount_paid] AS [paidAmount]
FROM [dbo].[Invoice] AS [i]
INNER JOIN [dbo].[Registration] AS [r]
    ON [r].[registration_id] = [i].[registration_id]
INNER JOIN [dbo].[Course] AS [c]
    ON [c].[course_id] = [r].[course_id]
LEFT JOIN [dbo].[Payment] AS [p]
    ON [p].[invoice_id] = [i].[invoice_id];
