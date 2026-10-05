CREATE OR ALTER PROCEDURE dbo.usp_GetClassGradeReview
    @classId int,
    @actorId int
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @@TRANCOUNT = 0 OR XACT_STATE() <> 1
        BEGIN
            THROW 51104, 'An active transaction with SNAPSHOT isolation is required.', 1;
        END;

        DECLARE @isolationLevel smallint;
        SELECT @isolationLevel = transaction_isolation_level
        FROM sys.dm_exec_sessions
        WHERE session_id = @@SPID;

        IF @isolationLevel IS NULL OR @isolationLevel <> 5
        BEGIN
            THROW 51104, 'An active transaction with SNAPSHOT isolation is required.', 1;
        END;

        IF @actorId IS NULL OR NOT EXISTS (
            SELECT 1
            FROM dbo.Account a
            INNER JOIN dbo.Role r ON r.role_id = a.role_id
            WHERE a.account_id = @actorId
              AND a.status = 'ACTIVE'
              AND a.must_change_password = 0
              AND r.role_code = 'AcademicAffairs'
        )
        BEGIN
            THROW 51101, 'Unauthorized or restricted account.', 1;
        END;

        IF @classId IS NULL OR NOT EXISTS (
            SELECT 1
            FROM dbo.Class c
            WHERE c.class_id = @classId
        )
        BEGIN
            THROW 51102, 'Class not found or no visible grade report.', 1;
        END;

        DECLARE @cohortSize int;
        DECLARE @visibleReportCount int;

        SELECT @cohortSize = COUNT(*)
        FROM dbo.StudentClassAssignment sca
        WHERE sca.class_id = @classId;

        SELECT @visibleReportCount = COUNT(*)
        FROM dbo.StudentClassAssignment sca
        INNER JOIN dbo.GradeReport gr ON gr.class_assignment_id = sca.class_assignment_id
        WHERE sca.class_id = @classId
          AND gr.status IN ('SUBMITTED', 'RETURNED', 'FINALIZED');

        IF @visibleReportCount = 0
        BEGIN
            THROW 51102, 'Class not found or no visible grade report.', 1;
        END;

        -- Result Set 1: Class Summary
        SELECT
            c.class_id AS classId,
            c.program_id AS programId,
            c.teacher_id AS teacherId,
            c.class_code AS classCode,
            c.status AS classStatus,
            CAST(c.row_version AS binary(8)) AS classVersion,
            CAST(tp.row_version AS binary(8)) AS programVersion,
            CAST(@cohortSize AS int) AS cohortSize,
            CAST(@visibleReportCount AS int) AS visibleReportCount
        FROM dbo.Class c
        INNER JOIN dbo.TrainingProgram tp ON tp.program_id = c.program_id
        WHERE c.class_id = @classId;

        -- Result Set 2: Score Component Configurations
        SELECT
            scc.component_id AS componentId,
            scc.component_name AS [name],
            scc.kind AS kind,
            scc.weight AS weight,
            CAST(scc.row_version AS binary(8)) AS [version]
        FROM dbo.ScoreComponentConfig scc
        INNER JOIN dbo.Class c ON c.program_id = scc.program_id
        WHERE c.class_id = @classId
        ORDER BY scc.component_id ASC;

        -- Result Set 3: Visible Grade Reports
        SELECT
            sca.class_assignment_id AS classAssignmentId,
            sca.registration_id AS registrationId,
            r.student_id AS studentId,
            gr.report_id AS reportId,
            a.full_name AS fullName,
            gr.status AS [status],
            gr.teacher_comment AS teacherComment,
            gr.submitted_at AS submittedAt,
            gr.reviewed_at AS reviewedAt,
            gr.finalized_at AS finalizedAt,
            gr.returned_reason AS latestReturnReason,
            gr.final_score AS finalScore,
            CAST(gr.row_version AS binary(8)) AS [version],
            fn.requiredComponentCount AS requiredComponentCount,
            fn.enteredComponentCount AS enteredComponentCount,
            fn.isComplete AS isComplete,
            fn.total AS total
        FROM dbo.StudentClassAssignment sca
        INNER JOIN dbo.GradeReport gr ON gr.class_assignment_id = sca.class_assignment_id
        INNER JOIN dbo.Registration r ON r.registration_id = sca.registration_id
        INNER JOIN dbo.StudentProfile sp ON sp.student_id = r.student_id
        INNER JOIN dbo.Account a ON a.account_id = sp.account_id
        CROSS APPLY dbo.ufn_GradeTotal(sca.class_assignment_id) fn
        WHERE sca.class_id = @classId
          AND gr.status IN ('SUBMITTED', 'RETURNED', 'FINALIZED')
        ORDER BY sca.class_assignment_id ASC;

        -- Result Set 4: Visible Saved Scores
        SELECT
            s.class_assignment_id AS classAssignmentId,
            s.score_id AS scoreId,
            s.component_id AS componentId,
            s.score_value AS score,
            s.schedule_id AS scheduleId,
            s.entered_by AS enteredByAccountId,
            s.entered_at AS enteredAt,
            CAST(s.row_version AS binary(8)) AS [version]
        FROM dbo.Score s
        INNER JOIN dbo.StudentClassAssignment sca ON sca.class_assignment_id = s.class_assignment_id
        INNER JOIN dbo.GradeReport gr ON gr.class_assignment_id = sca.class_assignment_id
        WHERE sca.class_id = @classId
          AND gr.status IN ('SUBMITTED', 'RETURNED', 'FINALIZED')
        ORDER BY s.class_assignment_id ASC, s.component_id ASC;

        -- Result Set 5: Linked Schedule References
        SELECT DISTINCT
            sch.schedule_id AS scheduleId,
            sch.class_id AS classId,
            sch.session_type AS [type],
            sch.obligation_key AS obligationKey,
            sch.session_date AS [date],
            sch.start_time AS startTime,
            sch.end_time AS endTime,
            sch.room AS room,
            sch.occurrence_state AS occurrenceState,
            CAST(sch.is_current AS bit) AS isCurrent,
            sch.replaces_schedule_id AS replacesScheduleId,
            CAST(sch.row_version AS binary(8)) AS [version]
        FROM dbo.Schedule sch
        WHERE sch.schedule_id IN (
            SELECT s.schedule_id
            FROM dbo.Score s
            INNER JOIN dbo.StudentClassAssignment sca ON sca.class_assignment_id = s.class_assignment_id
            INNER JOIN dbo.GradeReport gr ON gr.class_assignment_id = sca.class_assignment_id
            WHERE sca.class_id = @classId
              AND gr.status IN ('SUBMITTED', 'RETURNED', 'FINALIZED')
              AND s.schedule_id IS NOT NULL
        )
        ORDER BY sch.schedule_id ASC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH;
END;
