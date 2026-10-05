CREATE OR ALTER FUNCTION dbo.ufn_InvoiceDebtFlags
(
    @status varchar(16),
    @dueDate date,
    @todayVn date
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        CAST(CASE
            WHEN @status = 'UNPAID' AND @dueDate < @todayVn THEN 1
            ELSE 0
        END AS bit) AS isOverdue,
        CAST(CASE
            WHEN @status = 'UNPAID' AND @dueDate < @todayVn THEN DATEDIFF(day, @dueDate, @todayVn)
            ELSE 0
        END AS int) AS daysOverdue
);
