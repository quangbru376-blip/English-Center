CREATE OR ALTER TRIGGER [dbo].[trg_Score_ComponentScheduleGradeGuard]
ON [dbo].[Score]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
        RETURN;

    -- Gate check (51041): Caller must prehold Exclusive Transaction lock on EC:CLASS:<id> and EC:PROGRAM:<id>
    IF EXISTS (
        SELECT 1
        FROM (
            SELECT class_assignment_id FROM inserted
            UNION
            SELECT class_assignment_id FROM deleted
        ) a
        LEFT JOIN dbo.StudentClassAssignment sca ON sca.class_assignment_id = a.class_assignment_id
        LEFT JOIN dbo.Class c ON c.class_id = sca.class_id
        WHERE sca.class_id IS NULL
           OR c.program_id IS NULL
           OR ISNULL(APPLOCK_MODE('public', 'EC:CLASS:' + CAST(sca.class_id AS varchar(20)), 'Transaction'), '') <> 'Exclusive'
           OR ISNULL(APPLOCK_MODE('public', 'EC:PROGRAM:' + CAST(c.program_id AS varchar(20)), 'Transaction'), '') <> 'Exclusive'
    )
    BEGIN
        THROW 51041, 'Caller must hold Exclusive Transaction lock on EC:CLASS and EC:PROGRAM.', 1;
    END;

    -- Invariant (51042): Reject any mutation when old or new assignment GradeReport is SUBMITTED or FINALIZED
    IF EXISTS (
        SELECT 1
        FROM (
            SELECT class_assignment_id FROM inserted
            UNION
            SELECT class_assignment_id FROM deleted
        ) a
        INNER JOIN dbo.GradeReport gr ON gr.class_assignment_id = a.class_assignment_id
        WHERE gr.status IN ('SUBMITTED', 'FINALIZED')
    )
    BEGIN
        THROW 51042, 'Cannot modify score when grade report is submitted or finalized.', 1;
    END;

    -- Invariant (51042): Inserted component must belong to assignment Class.program_id
    IF EXISTS (
        SELECT 1
        FROM inserted i
        LEFT JOIN dbo.StudentClassAssignment sca ON sca.class_assignment_id = i.class_assignment_id
        LEFT JOIN dbo.Class c ON c.class_id = sca.class_id
        LEFT JOIN dbo.ScoreComponentConfig scc ON scc.component_id = i.component_id
        WHERE scc.program_id IS NULL
           OR c.program_id IS NULL
           OR scc.program_id <> c.program_id
    )
    BEGIN
        THROW 51042, 'Score component does not belong to the class training program.', 1;
    END;

    -- Invariant (51042): GENERAL component requires NULL schedule
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.ScoreComponentConfig scc ON scc.component_id = i.component_id
        WHERE scc.kind = 'GENERAL'
          AND i.schedule_id IS NOT NULL
    )
    BEGIN
        THROW 51042, 'General score component must not have a schedule.', 1;
    END;

    -- Invariant (51042): LESSON and FINAL_EXAM components require a valid current same-class schedule of matching session_type and occurrence_state <> NOT_HELD
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.ScoreComponentConfig scc ON scc.component_id = i.component_id
        LEFT JOIN dbo.StudentClassAssignment sca ON sca.class_assignment_id = i.class_assignment_id
        LEFT JOIN dbo.Schedule s ON s.schedule_id = i.schedule_id
        WHERE scc.kind IN ('LESSON', 'FINAL_EXAM')
          AND (
              i.schedule_id IS NULL
              OR s.schedule_id IS NULL
              OR sca.class_id IS NULL
              OR s.class_id <> sca.class_id
              OR s.is_current <> 1
              OR s.session_type <> scc.kind
              OR s.occurrence_state = 'NOT_HELD'
          )
    )
    BEGIN
        THROW 51042, 'Lesson and final exam score components require a valid current matching schedule.', 1;
    END;
END;
