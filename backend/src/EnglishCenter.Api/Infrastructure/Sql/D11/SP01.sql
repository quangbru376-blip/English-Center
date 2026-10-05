CREATE OR ALTER PROCEDURE dbo.usp_SearchAdmissions
    @actorId int, @courseId int, @status varchar(32),
    @createdFromUtc datetime2(3), @createdToExclusiveUtc datetime2(3),
    @name nvarchar(200), @email varchar(254), @phone varchar(10),
    @sort varchar(16), @page int, @pageSize int
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF XACT_STATE()<>1 OR NOT EXISTS(SELECT 1 FROM sys.dm_exec_sessions WHERE session_id=@@SPID AND transaction_isolation_level=5)
            THROW 51104,'Caller SNAPSHOT transaction required.',1;
        IF NOT EXISTS(SELECT 1 FROM dbo.Account a JOIN dbo.Role r ON r.role_id=a.role_id
            WHERE a.account_id=@actorId AND a.status='ACTIVE' AND a.must_change_password=0 AND r.role_code='Admissions')
            THROW 51101,'Unauthorized or restricted actor.',1;
        IF @page IS NULL OR @page<1 OR @pageSize IS NULL OR @pageSize NOT BETWEEN 1 AND 100
            OR @sort IS NULL OR @sort COLLATE Latin1_General_100_BIN2 NOT IN('CREATED_ASC','CREATED_DESC') OR DATALENGTH(@sort)<>LEN(@sort)
            OR (@courseId IS NOT NULL AND @courseId<1)
            OR (@createdFromUtc IS NOT NULL AND @createdToExclusiveUtc IS NOT NULL AND @createdFromUtc>=@createdToExclusiveUtc)
            OR (@status IS NOT NULL AND (@status COLLATE Latin1_General_100_BIN2 NOT IN('PENDING','TESTING','BAND_ASSIGNED','CONFIRMED','ADMITTED','REJECTED','CANCELLED') OR DATALENGTH(@status)<>LEN(@status)))
            THROW 51103,'Invalid filters or pagination.',1;
        DECLARE @offset bigint=(CONVERT(bigint,@page)-1)*@pageSize;
        DECLARE @namePattern nvarchar(402)=N'%'+REPLACE(REPLACE(REPLACE(REPLACE(@name,N'~',N'~~'),N'%',N'~%'),N'_',N'~_'),N'[',N'~[')+N'%';
        DECLARE @emailPattern varchar(512)='%'+REPLACE(REPLACE(REPLACE(REPLACE(@email,'~','~~'),'%','~%'),'_','~_'),'[','~[')+'%';
        DECLARE @phonePattern varchar(22)='%'+REPLACE(REPLACE(REPLACE(REPLACE(@phone,'~','~~'),'%','~%'),'_','~_'),'[','~[')+'%';
        SELECT registrationId,studentId,fullName,email,phone,courseId,courseCode,courseName,courseStatus,
            registrationStatus,createdAt,admissionConfirmedAt,admissionApprovedAt,bandId,bandCode,bandMinScore,
            invoiceId,invoiceNumber,invoiceAmount,invoiceStatus,classId,assignmentId,classCode,classStatus,
            CONVERT(binary(8),registrationVersion) AS registrationVersion
        INTO #Matches FROM dbo.vw_AdmissionsRegistrationFacts
        WHERE (@courseId IS NULL OR courseId=@courseId) AND (@status IS NULL OR registrationStatus=@status)
            AND (@createdFromUtc IS NULL OR createdAt>=@createdFromUtc) AND (@createdToExclusiveUtc IS NULL OR createdAt<@createdToExclusiveUtc)
            AND (@name IS NULL OR fullName LIKE @namePattern ESCAPE '~')
            AND (@email IS NULL OR email LIKE @emailPattern ESCAPE '~')
            AND (@phone IS NULL OR phone LIKE @phonePattern ESCAPE '~');
        SELECT COUNT_BIG(*) AS totalCount FROM #Matches;
        SELECT registrationId,studentId,fullName,email,phone,courseId,courseCode,courseName,courseStatus,
            registrationStatus,createdAt,admissionConfirmedAt,admissionApprovedAt,bandId,bandCode,bandMinScore,
            invoiceId,invoiceNumber,invoiceAmount,invoiceStatus,classId,assignmentId,classCode,classStatus,registrationVersion
        FROM #Matches
        ORDER BY CASE WHEN @sort='CREATED_ASC' THEN createdAt END ASC,
            CASE WHEN @sort='CREATED_DESC' THEN createdAt END DESC,
            CASE WHEN @sort='CREATED_ASC' THEN registrationId END ASC,
            CASE WHEN @sort='CREATED_DESC' THEN registrationId END DESC
        OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH;
END;
