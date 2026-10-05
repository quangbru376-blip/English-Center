CREATE OR ALTER PROCEDURE dbo.usp_GetFinancialReport
    @fromUtc datetime2(3),
    @toExclusiveUtc datetime2(3),
    @courseId int = NULL,
    @overdueOnly bit,
    @todayVn date,
    @page int = NULL,
    @pageSize int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF XACT_STATE() <> 1
            OR NOT EXISTS (
                SELECT 1
                FROM sys.dm_exec_sessions
                WHERE session_id = @@SPID
                  AND transaction_isolation_level = 5
            )
        BEGIN
            THROW 51104, 'An active transaction with snapshot isolation is required.', 1;
        END;

        IF @fromUtc IS NULL
            OR @toExclusiveUtc IS NULL
            OR @fromUtc >= @toExclusiveUtc
            OR @todayVn IS NULL
            OR @overdueOnly IS NULL
            OR (@courseId IS NOT NULL AND @courseId <= 0)
            OR (@page IS NULL AND @pageSize IS NOT NULL)
            OR (@page IS NOT NULL AND @pageSize IS NULL)
            OR (@page IS NOT NULL AND (@page < 1 OR @pageSize < 1 OR @pageSize > 100))
        BEGIN
            THROW 51103, 'Invalid filter, pagination, or date range parameters.', 1;
        END;

        DECLARE @CourseTotals TABLE
        (
            courseId int NOT NULL PRIMARY KEY,
            courseCode varchar(30) NOT NULL,
            courseName nvarchar(300) NOT NULL,
            revenueAmount decimal(38,0) NOT NULL,
            billedAmount decimal(38,0) NOT NULL,
            currentDebtAmount decimal(38,0) NOT NULL,
            overdueDebtAmount decimal(38,0) NOT NULL
        );

        INSERT INTO @CourseTotals
        (
            courseId,
            courseCode,
            courseName,
            revenueAmount,
            billedAmount,
            currentDebtAmount,
            overdueDebtAmount
        )
        SELECT
            u.course_id,
            c.course_code,
            c.course_name,
            SUM(u.revenueAmount) AS revenueAmount,
            SUM(u.billedAmount) AS billedAmount,
            SUM(u.currentDebtAmount) AS currentDebtAmount,
            SUM(u.overdueDebtAmount) AS overdueDebtAmount
        FROM
        (
            SELECT v.courseId AS course_id,
                SUM(CONVERT(decimal(38,0),v.paidAmount)) AS revenueAmount,
                CAST(0 AS decimal(38,0)) AS billedAmount,
                CAST(0 AS decimal(38,0)) AS currentDebtAmount,
                CAST(0 AS decimal(38,0)) AS overdueDebtAmount
            FROM dbo.vw_FinanceInvoiceFacts v
            WHERE v.paidAt>=@fromUtc AND v.paidAt<@toExclusiveUtc
                AND (@courseId IS NULL OR v.courseId=@courseId)
            GROUP BY v.courseId
            UNION ALL
            SELECT v.courseId,CAST(0 AS decimal(38,0)),SUM(CONVERT(decimal(38,0),v.amount)),
                CAST(0 AS decimal(38,0)),CAST(0 AS decimal(38,0))
            FROM dbo.vw_FinanceInvoiceFacts v
            WHERE v.invoiceStatus IN('UNPAID','PAID') AND v.issuedAt>=@fromUtc AND v.issuedAt<@toExclusiveUtc
                AND (@courseId IS NULL OR v.courseId=@courseId)
            GROUP BY v.courseId
            UNION ALL
            SELECT v.courseId,CAST(0 AS decimal(38,0)),CAST(0 AS decimal(38,0)),
                SUM(CONVERT(decimal(38,0),v.amount)),
                SUM(CONVERT(decimal(38,0),CASE WHEN fn.isOverdue=1 THEN v.amount ELSE 0 END))
            FROM dbo.vw_FinanceInvoiceFacts v
            CROSS APPLY dbo.ufn_InvoiceDebtFlags(v.invoiceStatus,v.dueDate,@todayVn) fn
            WHERE v.invoiceStatus='UNPAID' AND (@overdueOnly=0 OR fn.isOverdue=1)
                AND (@courseId IS NULL OR v.courseId=@courseId)
            GROUP BY v.courseId
        ) u
        INNER JOIN dbo.Course c ON c.course_id = u.course_id
        GROUP BY u.course_id, c.course_code, c.course_name;

        -- Result Set 1: Overall Totals (exactly one row)
        SELECT
            CAST(COUNT_BIG(1) AS bigint) AS totalCount,
            COALESCE(CONVERT(varchar(38), SUM(revenueAmount)), '0') AS revenueAmount,
            COALESCE(CONVERT(varchar(38), SUM(billedAmount)), '0') AS billedAmount,
            COALESCE(CONVERT(varchar(38), SUM(currentDebtAmount)), '0') AS currentDebtAmount,
            COALESCE(CONVERT(varchar(38), SUM(overdueDebtAmount)), '0') AS overdueDebtAmount
        FROM @CourseTotals;

        -- Result Set 2: Course Groups (paged or full CSV)
        IF @page IS NULL
        BEGIN
            SELECT
                courseId,
                courseCode,
                courseName,
                CONVERT(varchar(38), revenueAmount) AS revenueAmount,
                CONVERT(varchar(38), billedAmount) AS billedAmount,
                CONVERT(varchar(38), currentDebtAmount) AS currentDebtAmount,
                CONVERT(varchar(38), overdueDebtAmount) AS overdueDebtAmount
            FROM @CourseTotals
            ORDER BY courseCode ASC, courseId ASC;
        END
        ELSE
        BEGIN
            SELECT
                courseId,
                courseCode,
                courseName,
                CONVERT(varchar(38), revenueAmount) AS revenueAmount,
                CONVERT(varchar(38), billedAmount) AS billedAmount,
                CONVERT(varchar(38), currentDebtAmount) AS currentDebtAmount,
                CONVERT(varchar(38), overdueDebtAmount) AS overdueDebtAmount
            FROM @CourseTotals
            ORDER BY courseCode ASC, courseId ASC
            OFFSET (CONVERT(bigint, @page) - 1) * CONVERT(bigint, @pageSize) ROWS
            FETCH NEXT @pageSize ROWS ONLY;
        END;

        -- Result Set 3: Course Options
        SELECT
            c.course_id AS courseId,
            c.course_code AS courseCode,
            c.course_name AS courseName,
            c.status AS status
        FROM dbo.Course c
        WHERE c.status = 'ACTIVE'
           OR (
               c.status = 'INACTIVE'
               AND (
                   EXISTS (
                       SELECT 1
                       FROM dbo.Registration r
                       WHERE r.course_id = c.course_id
                   )
                   OR EXISTS (
                       SELECT 1
                       FROM dbo.TrainingProgram tp
                       INNER JOIN dbo.Class cl ON cl.program_id = tp.program_id
                       WHERE tp.course_id = c.course_id
                   )
               )
           )
        ORDER BY c.course_code ASC, c.course_id ASC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH;
END;
