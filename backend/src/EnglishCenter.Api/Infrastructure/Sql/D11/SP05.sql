CREATE OR ALTER PROCEDURE [dbo].[usp_GetAdmissionsReport]
    @fromUtc datetime2(3),
    @toExclusiveUtc datetime2(3),
    @courseId int = NULL,
    @status varchar(16) = NULL,
    @page int,
    @pageSize int
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @@TRANCOUNT = 0 OR XACT_STATE() <> 1
        BEGIN
            THROW 51104, 'An active caller transaction with SNAPSHOT isolation is required.', 1;
        END

        DECLARE @isolationLevel smallint;
        SELECT @isolationLevel = transaction_isolation_level
        FROM sys.dm_exec_sessions
        WHERE session_id = @@SPID;

        IF @isolationLevel IS NULL OR @isolationLevel <> 5
        BEGIN
            THROW 51104, 'An active caller transaction with SNAPSHOT isolation is required.', 1;
        END

        IF @fromUtc IS NULL
           OR @toExclusiveUtc IS NULL
           OR @fromUtc >= @toExclusiveUtc
           OR @page IS NULL
           OR @page < 1
           OR @pageSize IS NULL
           OR @pageSize < 1
           OR @pageSize > 100
           OR (@courseId IS NOT NULL AND @courseId < 1)
           OR (@status IS NOT NULL AND (@status COLLATE Latin1_General_100_BIN2 NOT IN ('PENDING', 'TESTING', 'BAND_ASSIGNED', 'CONFIRMED', 'ADMITTED', 'REJECTED', 'CANCELLED') OR DATALENGTH(@status) <> LEN(@status)))
        BEGIN
            THROW 51103, 'Invalid filter or pagination parameters.', 1;
        END

        DECLARE @offset bigint = (CONVERT(bigint, @page) - 1) * CONVERT(bigint, @pageSize);

        DECLARE @Cohort TABLE (
            registration_id int NOT NULL,
            student_id int NOT NULL,
            course_id int NOT NULL
        );

        INSERT INTO @Cohort (registration_id,student_id,course_id)
        SELECT registrationId,studentId,courseId FROM dbo.vw_AdmissionsRegistrationFacts
        WHERE createdAt>=@fromUtc AND createdAt<@toExclusiveUtc
            AND (@courseId IS NULL OR courseId=@courseId)
            AND (@status IS NULL OR registrationStatus=@status);

        -- RS1: Overall cohort summary (single row)
        SELECT
            COUNT_BIG(DISTINCT ch.course_id) AS totalCount,
            COUNT_BIG(*) AS registrationCount,
            COUNT_BIG(DISTINCT ch.student_id) AS distinctStudentCount
        FROM @Cohort ch;

        -- RS2: Course groups breakdown (paginated, ordered by courseCode/courseId)
        SELECT
            c.course_id AS courseId,
            c.course_code AS courseCode,
            c.course_name AS courseName,
            COUNT_BIG(*) AS registrationCount,
            COUNT_BIG(DISTINCT ch.student_id) AS distinctStudentCount
        FROM @Cohort ch
        INNER JOIN dbo.Course c ON c.course_id = ch.course_id
        GROUP BY c.course_id, c.course_code, c.course_name
        ORDER BY c.course_code ASC, c.course_id ASC
        OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;

        -- RS3: Course options (active + referenced inactive courses)
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
                   EXISTS (SELECT 1 FROM dbo.Registration r WHERE r.course_id = c.course_id)
                   OR EXISTS (SELECT 1 FROM dbo.TrainingProgram tp JOIN dbo.Class cl ON cl.program_id=tp.program_id WHERE tp.course_id = c.course_id)
               )
           )
        ORDER BY c.course_code ASC, c.course_id ASC;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
