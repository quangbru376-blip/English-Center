CREATE OR ALTER PROCEDURE [dbo].[usp_GetTrainingReport]
    @courseId int = NULL,
    @bandId int = NULL,
    @classId int = NULL,
    @teacherId int = NULL,
    @status varchar(16) = NULL,
    @finalizedFromUtc datetime2(3) = NULL,
    @finalizedToExclusiveUtc datetime2(3) = NULL,
    @page int = NULL,
    @pageSize int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF XACT_STATE() <> 1
        OR NOT EXISTS (
            SELECT 1
            FROM sys.dm_exec_sessions AS s
            WHERE s.session_id = @@SPID
              AND s.transaction_isolation_level = 5
        )
    BEGIN
        THROW 51104, N'Caller transaction must be active with SNAPSHOT isolation.', 1;
    END;

    IF @finalizedFromUtc IS NULL
        OR @finalizedToExclusiveUtc IS NULL
        OR @finalizedFromUtc >= @finalizedToExclusiveUtc
        OR @page IS NULL
        OR @page < 1
        OR @pageSize IS NULL
        OR @pageSize < 1
        OR @pageSize > 100
        OR (@status IS NOT NULL AND @status COLLATE Latin1_General_100_BIN2 NOT IN ('PLANNED', 'OPEN', 'ONGOING', 'COMPLETED', 'CANCELLED'))
        OR (@status IS NOT NULL AND DATALENGTH(@status)<>LEN(@status))
        OR (@courseId IS NOT NULL AND @courseId < 1)
        OR (@bandId IS NOT NULL AND @bandId < 1)
        OR (@classId IS NOT NULL AND @classId < 1)
        OR (@teacherId IS NOT NULL AND @teacherId < 1)
    BEGIN
        THROW 51103, N'Invalid filters, page, or date range.', 1;
    END;

    BEGIN TRY
        DECLARE @offset bigint = (CONVERT(bigint, @page) - 1) * CONVERT(bigint, @pageSize);

        CREATE TABLE #MatchedClasses (
            classId int NOT NULL PRIMARY KEY,
            courseId int NOT NULL,
            bandId int NOT NULL,
            teacherId int NOT NULL,
            classCode varchar(30) NOT NULL,
            courseCode varchar(30) NOT NULL,
            courseName nvarchar(300) NOT NULL,
            bandCode varchar(30) NOT NULL,
            bandMinScore decimal(5,2) NOT NULL,
            classStatus varchar(16) NOT NULL,
            maxSize int NOT NULL,
            placedCount int NOT NULL,
            expectedCount int NOT NULL,
            heldCount bigint NOT NULL,
            examCurrentCount int NOT NULL,
            examHeldCount int NOT NULL
        );

        INSERT INTO #MatchedClasses (
            classId,
            courseId,
            bandId,
            teacherId,
            classCode,
            courseCode,
            courseName,
            bandCode,
            bandMinScore,
            classStatus,
            maxSize,
            placedCount,
            expectedCount,
            heldCount,
            examCurrentCount,
            examHeldCount
        )
        SELECT
            f.classId,
            f.courseId,
            f.bandId,
            f.teacherId,
            f.classCode,
            f.courseCode,
            f.courseName,
            f.bandCode,
            f.bandMinScore,
            f.classStatus,
            f.maxSize,
            f.placedCount,
            f.expectedCount,
            f.heldCount,
            f.examCurrentCount,
            f.examHeldCount
        FROM dbo.vw_TrainingClassFacts AS f
        WHERE (@courseId IS NULL OR f.courseId = @courseId)
          AND (@bandId IS NULL OR f.bandId = @bandId)
          AND (@classId IS NULL OR f.classId = @classId)
          AND (@teacherId IS NULL OR f.teacherId = @teacherId)
          AND (@status IS NULL OR f.classStatus COLLATE Latin1_General_100_BIN2 = @status COLLATE Latin1_General_100_BIN2);

        CREATE TABLE #ClassGradeAggregates (
            classId int NOT NULL PRIMARY KEY,
            finalizedResultCount bigint NOT NULL,
            passingResultCount bigint NOT NULL
        );

        INSERT INTO #ClassGradeAggregates (
            classId,
            finalizedResultCount,
            passingResultCount
        )
        SELECT
            g.classId,
            COUNT_BIG(*) AS finalizedResultCount,
            COUNT_BIG(CASE WHEN g.finalScore >= 50.00 THEN 1 END) AS passingResultCount
        FROM dbo.vw_FinalizedGradeFacts AS g
        JOIN #MatchedClasses AS m ON m.classId = g.classId
        WHERE g.finalizedAt >= @finalizedFromUtc
          AND g.finalizedAt < @finalizedToExclusiveUtc
        GROUP BY g.classId;

        -- RS1: Summary metrics across full matched class set
        SELECT
            CAST(COUNT_BIG(*) AS bigint) AS totalCount,
            CAST(COUNT_BIG(*) AS bigint) AS classCount,
            CAST(COUNT_BIG(CASE WHEN m.classStatus = 'PLANNED' THEN 1 END) AS bigint) AS plannedCount,
            CAST(COUNT_BIG(CASE WHEN m.classStatus = 'OPEN' THEN 1 END) AS bigint) AS openCount,
            CAST(COUNT_BIG(CASE WHEN m.classStatus = 'ONGOING' THEN 1 END) AS bigint) AS ongoingCount,
            CAST(COUNT_BIG(CASE WHEN m.classStatus = 'COMPLETED' THEN 1 END) AS bigint) AS completedCount,
            CAST(COUNT_BIG(CASE WHEN m.classStatus = 'CANCELLED' THEN 1 END) AS bigint) AS cancelledCount,
            CAST(COALESCE(SUM(a.finalizedResultCount), CAST(0 AS bigint)) AS bigint) AS finalizedResultCount,
            CAST(COALESCE(SUM(a.passingResultCount), CAST(0 AS bigint)) AS bigint) AS passingResultCount,
            CASE
                WHEN COALESCE(SUM(a.finalizedResultCount), CAST(0 AS bigint)) = 0 THEN CAST(NULL AS decimal(5,2))
                ELSE CAST(ROUND(CONVERT(decimal(28,8), SUM(a.passingResultCount)) * 100.0 / CONVERT(decimal(28,8), SUM(a.finalizedResultCount)), 2) AS decimal(5,2))
            END AS passRatePercent
        FROM #MatchedClasses AS m
        LEFT JOIN #ClassGradeAggregates AS a ON a.classId = m.classId;

        -- RS2: Paged matched-Class rows with inventory and finalized results
        SELECT
            m.classId,
            m.courseId,
            m.bandId,
            m.classCode,
            m.courseCode,
            m.courseName,
            m.bandCode,
            m.bandMinScore,
            m.classStatus,
            m.maxSize,
            m.placedCount,
            m.expectedCount,
            CASE
                WHEN m.classStatus = 'CANCELLED' THEN CAST(NULL AS bigint)
                ELSE m.heldCount
            END AS heldCount,
            CASE
                WHEN m.classStatus = 'CANCELLED' OR m.expectedCount <= 0 THEN CAST(NULL AS decimal(5,2))
                ELSE CAST(ROUND(CONVERT(decimal(28,8), m.heldCount) * 100.0 / CONVERT(decimal(28,8), m.expectedCount), 2) AS decimal(5,2))
            END AS progressPercent,
            CASE
                WHEN m.classStatus = 'CANCELLED' THEN CAST(NULL AS int)
                ELSE m.examCurrentCount
            END AS examCurrentCount,
            CASE
                WHEN m.classStatus = 'CANCELLED' THEN CAST(NULL AS int)
                ELSE m.examHeldCount
            END AS examHeldCount,
            CAST(COALESCE(a.finalizedResultCount, CAST(0 AS bigint)) AS bigint) AS finalizedResultCount,
            CAST(COALESCE(a.passingResultCount, CAST(0 AS bigint)) AS bigint) AS passingResultCount,
            CASE
                WHEN COALESCE(a.finalizedResultCount, CAST(0 AS bigint)) = 0 THEN CAST(NULL AS decimal(5,2))
                ELSE CAST(ROUND(CONVERT(decimal(28,8), a.passingResultCount) * 100.0 / CONVERT(decimal(28,8), a.finalizedResultCount), 2) AS decimal(5,2))
            END AS passRatePercent
        FROM #MatchedClasses AS m
        LEFT JOIN #ClassGradeAggregates AS a ON a.classId = m.classId
        ORDER BY m.classCode ASC, m.classId ASC
        OFFSET @offset ROWS
        FETCH NEXT @pageSize ROWS ONLY;

        DROP TABLE IF EXISTS #ClassGradeAggregates;
        DROP TABLE IF EXISTS #MatchedClasses;

        -- RS3: Full Courses option set (Active + referenced Inactive)
        SELECT
            c.course_id AS courseId,
            c.course_code AS courseCode,
            c.course_name AS courseName,
            c.status AS status
        FROM dbo.Course AS c
        WHERE c.status = 'ACTIVE'
           OR (
               c.status = 'INACTIVE'
               AND (
                   EXISTS (SELECT 1 FROM dbo.Registration AS r WHERE r.course_id = c.course_id)
                   OR EXISTS (
                       SELECT 1
                       FROM dbo.Class AS cl
                       JOIN dbo.TrainingProgram AS tp ON tp.program_id = cl.program_id
                       WHERE tp.course_id = c.course_id
                   )
               )
           )
        ORDER BY c.course_code ASC, c.course_id ASC;

        -- RS4: Full Bands option set
        SELECT
            b.band_id AS bandId,
            b.band_code AS bandCode,
            b.min_score AS minScore
        FROM dbo.Band AS b
        ORDER BY b.band_code ASC, b.band_id ASC;

        -- RS5: Full Classes option set
        SELECT
            cl.class_id AS classId,
            tp.course_id AS courseId,
            tp.band_id AS bandId,
            cl.class_code AS classCode
        FROM dbo.Class AS cl
        JOIN dbo.TrainingProgram AS tp ON tp.program_id = cl.program_id
        ORDER BY cl.class_code ASC, cl.class_id ASC;

        -- RS6: Referenced current Class Teachers option set
        SELECT
            a.account_id AS teacherId,
            a.full_name AS displayName
        FROM dbo.Account AS a
        WHERE EXISTS (
            SELECT 1
            FROM dbo.Class AS cl
            WHERE cl.teacher_id = a.account_id
        )
        ORDER BY a.full_name ASC, a.account_id ASC;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH;
END;
