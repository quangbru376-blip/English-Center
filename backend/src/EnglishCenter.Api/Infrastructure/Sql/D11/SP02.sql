CREATE OR ALTER PROCEDURE dbo.usp_GetEntryTestRoster
    @sessionId int,
    @actorId int,
    @asOfUtc datetime2(3),
    @name nvarchar(200) = NULL,
    @email varchar(254) = NULL,
    @phone varchar(10) = NULL,
    @bookingStatus varchar(16) = NULL,
    @view varchar(8),
    @sort varchar(16),
    @page int,
    @pageSize int
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @@TRANCOUNT = 0 OR XACT_STATE() <> 1
        BEGIN
            THROW 51104, N'Active caller transaction is required.', 1;
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.dm_exec_sessions
            WHERE session_id = @@SPID AND transaction_isolation_level = 5
        )
        BEGIN
            THROW 51104, N'Snapshot transaction isolation level is required.', 1;
        END;

        IF @sessionId IS NULL
           OR @actorId IS NULL
           OR @asOfUtc IS NULL
           OR @view IS NULL
           OR @view COLLATE Latin1_General_100_BIN2 NOT IN ('current', 'history')
           OR DATALENGTH(@view)<>LEN(@view)
           OR DATALENGTH(@sort)<>LEN(@sort)
           OR (@bookingStatus IS NOT NULL AND DATALENGTH(@bookingStatus)<>LEN(@bookingStatus))
           OR @sort IS NULL
           OR @sort COLLATE Latin1_General_100_BIN2 NOT IN ('BOOKED_ASC', 'BOOKED_DESC')
           OR @page IS NULL
           OR @page < 1
           OR @pageSize IS NULL
           OR @pageSize < 1
           OR @pageSize > 100
           OR (@bookingStatus IS NOT NULL AND @bookingStatus COLLATE Latin1_General_100_BIN2 NOT IN ('SCHEDULED', 'COMPLETED', 'ABSENT', 'CANCELLED'))
        BEGIN
            THROW 51103, N'Invalid entry test roster parameters.', 1;
        END;

        DECLARE @roleCode varchar(32);
        DECLARE @accountStatus varchar(16);
        DECLARE @mustChangePassword bit;

        SELECT
            @roleCode = r.role_code,
            @accountStatus = a.status,
            @mustChangePassword = a.must_change_password
        FROM dbo.Account a
        JOIN dbo.Role r ON r.role_id = a.role_id
        WHERE a.account_id = @actorId;

        IF @roleCode IS NULL
           OR @accountStatus <> 'ACTIVE'
           OR @mustChangePassword <> 0
           OR @roleCode NOT IN ('AcademicAffairs', 'Teacher')
        BEGIN
            THROW 51101, N'Unauthorized active role or restricted account.', 1;
        END;

        DECLARE @sessionGraderId int;
        DECLARE @effectiveSessionStatus varchar(16);

        SELECT
            @sessionGraderId = s.grader_id,
            @effectiveSessionStatus = dbo.ufn_EffectiveEntryTestState(s.status, s.start_at, @asOfUtc)
        FROM dbo.EntryTestSession s
        WHERE s.session_id = @sessionId;

        IF @sessionGraderId IS NULL
        BEGIN
            THROW 51102, N'Entry test session not found.', 1;
        END;

        IF @roleCode = 'Teacher' AND @sessionGraderId <> @actorId
        BEGIN
            THROW 51102, N'Entry test session not found or not assigned.', 1;
        END;

        DECLARE @offset bigint = (CONVERT(bigint, @page) - 1) * CONVERT(bigint, @pageSize);

        DECLARE @escapedName nvarchar(402);
        IF @name IS NOT NULL
        BEGIN
            SET @escapedName = N'%' + REPLACE(REPLACE(REPLACE(REPLACE(@name, N'~', N'~~'), N'%', N'~%'), N'_', N'~_'), N'[', N'~[') + N'%';
        END;

        DECLARE @escapedEmail varchar(512);
        IF @email IS NOT NULL
        BEGIN
            SET @escapedEmail = '%' + REPLACE(REPLACE(REPLACE(REPLACE(@email, '~', '~~'), '%', '~%'), '_', '~_'), '[', '~[') + '%';
        END;

        DECLARE @escapedPhone varchar(30);
        IF @phone IS NOT NULL
        BEGIN
            SET @escapedPhone = '%' + REPLACE(REPLACE(REPLACE(REPLACE(@phone, '~', '~~'), '%', '~%'), '_', '~_'), '[', '~[') + '%';
        END;

        -- Result Set 1: Total Count
        SELECT CAST(COUNT_BIG(*) AS bigint) AS totalCount
        FROM dbo.vw_EntryTestBookingFacts v
        WHERE v.sessionId = @sessionId
          AND (@view = 'history' OR v.bookingStatus = 'SCHEDULED')
          AND (@bookingStatus IS NULL OR v.bookingStatus = @bookingStatus)
          AND (@escapedName IS NULL OR v.fullName LIKE @escapedName ESCAPE '~')
          AND (@escapedEmail IS NULL OR v.email LIKE @escapedEmail ESCAPE '~')
          AND (@escapedPhone IS NULL OR v.phone LIKE @escapedPhone ESCAPE '~');

        -- Result Set 2: Paged Roster Rows
        IF @roleCode = 'Teacher'
        BEGIN
            SELECT
                v.bookingId,
                v.attemptId,
                v.registrationId,
                v.studentId,
                v.courseId,
                v.attemptNumber,
                v.bookedAt,
                v.bookingStatus,
                v.registrationStatus,
                v.fullName,
                v.email,
                v.phone,
                v.courseCode,
                v.courseName,
                v.courseStatus,
                v.targetScore,
                v.targetGradedAt,
                v.bandId,
                v.bandCode,
                v.bandMinScore
            FROM dbo.vw_EntryTestBookingFacts v
            WHERE v.sessionId = @sessionId
              AND (@view = 'history' OR v.bookingStatus = 'SCHEDULED')
              AND (@bookingStatus IS NULL OR v.bookingStatus = @bookingStatus)
              AND (@escapedName IS NULL OR v.fullName LIKE @escapedName ESCAPE '~')
              AND (@escapedEmail IS NULL OR v.email LIKE @escapedEmail ESCAPE '~')
              AND (@escapedPhone IS NULL OR v.phone LIKE @escapedPhone ESCAPE '~')
            ORDER BY
                CASE WHEN @sort = 'BOOKED_ASC' THEN v.bookedAt END ASC,
                CASE WHEN @sort = 'BOOKED_DESC' THEN v.bookedAt END DESC,
                CASE WHEN @sort = 'BOOKED_ASC' THEN v.bookingId END ASC,
                CASE WHEN @sort = 'BOOKED_DESC' THEN v.bookingId END DESC
            OFFSET @offset ROWS
            FETCH NEXT @pageSize ROWS ONLY;
        END
        ELSE
        BEGIN
            SELECT
                v.bookingId,
                v.attemptId,
                v.registrationId,
                v.studentId,
                v.courseId,
                v.attemptNumber,
                v.bookedAt,
                v.bookingStatus,
                v.registrationStatus,
                v.fullName,
                v.email,
                v.phone,
                v.courseCode,
                v.courseName,
                v.courseStatus,
                v.targetScore,
                v.targetGradedAt,
                v.bandId,
                v.bandCode,
                v.bandMinScore,
                v.invoiceStatus
            FROM dbo.vw_EntryTestBookingFacts v
            WHERE v.sessionId = @sessionId
              AND (@view = 'history' OR v.bookingStatus = 'SCHEDULED')
              AND (@bookingStatus IS NULL OR v.bookingStatus = @bookingStatus)
              AND (@escapedName IS NULL OR v.fullName LIKE @escapedName ESCAPE '~')
              AND (@escapedEmail IS NULL OR v.email LIKE @escapedEmail ESCAPE '~')
              AND (@escapedPhone IS NULL OR v.phone LIKE @escapedPhone ESCAPE '~')
            ORDER BY
                CASE WHEN @sort = 'BOOKED_ASC' THEN v.bookedAt END ASC,
                CASE WHEN @sort = 'BOOKED_DESC' THEN v.bookedAt END DESC,
                CASE WHEN @sort = 'BOOKED_ASC' THEN v.bookingId END ASC,
                CASE WHEN @sort = 'BOOKED_DESC' THEN v.bookingId END DESC
            OFFSET @offset ROWS
            FETCH NEXT @pageSize ROWS ONLY;
        END;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH;
END;
