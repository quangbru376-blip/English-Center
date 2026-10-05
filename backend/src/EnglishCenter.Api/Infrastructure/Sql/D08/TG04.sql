CREATE OR ALTER TRIGGER [dbo].[trg_Attendance_ClassScheduleGuard]
ON [dbo].[Attendance]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
        RETURN;

    -- Gate check (51031):
    -- Caller must prehold Exclusive Transaction public locks on EC:CLASS:<id> and EC:PROGRAM:<program_id>.
    IF EXISTS (
        SELECT 1
        FROM (
            SELECT DISTINCT
                sca.class_id,
                c.program_id
            FROM (
                SELECT class_assignment_id FROM inserted
                UNION
                SELECT class_assignment_id FROM deleted
            ) a
            LEFT JOIN [dbo].[StudentClassAssignment] sca ON sca.class_assignment_id = a.class_assignment_id
            LEFT JOIN [dbo].[Class] c ON c.class_id = sca.class_id
        ) ac
        WHERE ac.class_id IS NULL
           OR ac.program_id IS NULL
           OR ISNULL(APPLOCK_MODE(N'public', N'EC:CLASS:' + CAST(ac.class_id AS nvarchar(20)), N'Transaction'), N'') <> N'Exclusive'
           OR ISNULL(APPLOCK_MODE(N'public', N'EC:PROGRAM:' + CAST(ac.program_id AS nvarchar(20)), N'Transaction'), N'') <> N'Exclusive'
    )
    BEGIN
        THROW 51031, 'Caller must hold exclusive transaction lock on EC:CLASS:<id> and EC:PROGRAM:<program_id>.', 1;
    END;

    -- Invariant check 1 (51032):
    -- Reject all mutations affecting old or new Class COMPLETED/CANCELLED.
    IF EXISTS (
        SELECT 1
        FROM (
            SELECT class_assignment_id FROM inserted
            UNION
            SELECT class_assignment_id FROM deleted
        ) a
        JOIN [dbo].[StudentClassAssignment] sca ON sca.class_assignment_id = a.class_assignment_id
        JOIN [dbo].[Class] c ON c.class_id = sca.class_id
        WHERE c.status IN ('COMPLETED', 'CANCELLED')
    )
    BEGIN
        THROW 51032, 'Cannot mutate attendance for completed or cancelled class.', 1;
    END;

    -- Invariant check 2 (51032):
    -- Every inserted row schedule must belong to assignment Class, is_current=1, session_type=LESSON, occurrence_state<>NOT_HELD.
    IF EXISTS (
        SELECT 1
        FROM inserted i
        LEFT JOIN [dbo].[StudentClassAssignment] sca ON sca.class_assignment_id = i.class_assignment_id
        LEFT JOIN [dbo].[Schedule] s ON s.schedule_id = i.schedule_id
        WHERE sca.class_assignment_id IS NULL
           OR s.schedule_id IS NULL
           OR s.class_id <> sca.class_id
           OR s.is_current <> 1
           OR s.session_type <> 'LESSON'
           OR s.occurrence_state = 'NOT_HELD'
    )
    BEGIN
        THROW 51032, 'Schedule must belong to assignment class, be current, of session type LESSON, and occurrence state not NOT_HELD.', 1;
    END;
END;
