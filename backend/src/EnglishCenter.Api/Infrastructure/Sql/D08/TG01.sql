CREATE OR ALTER TRIGGER [dbo].[trg_Program_UsedConfigurationGuard]
ON [dbo].[TrainingProgram]
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Gate check (51001): caller must already hold Exclusive Transaction applock
    -- public EC:PROGRAM:<id> for every affected deleted/configuration-modified Program.
    -- Trigger must verify existing locks via APPLOCK_MODE and never acquire gates after DML.
    IF EXISTS (
        SELECT 1
        FROM deleted d
        LEFT JOIN inserted i ON i.program_id = d.program_id
        WHERE (
            APPLOCK_MODE('public', 'EC:PROGRAM:' + CAST(d.program_id AS varchar(11)), 'Transaction') IS NULL
            OR APPLOCK_MODE('public', 'EC:PROGRAM:' + CAST(d.program_id AS varchar(11)), 'Transaction') <> 'Exclusive'
        )
    )
    BEGIN
        THROW 51001, 'Exclusive transaction applock public EC:PROGRAM:<id> required.', 1;
    END;

    -- Invariant check (51002): for any deleted row whose program has ANY Class (including CANCELLED),
    -- reject deleted rows missing in inserted or changing course_id, band_id, expected_lesson_count.
    -- Allow unchanged core config including valid name/status update and value-compared no-op updates.
    IF EXISTS (
        SELECT 1
        FROM deleted d
        LEFT JOIN inserted i ON i.program_id = d.program_id
        WHERE EXISTS (
            SELECT 1
            FROM [dbo].[Class] c
            WHERE c.program_id = d.program_id
        )
        AND (
            i.program_id IS NULL
            OR d.course_id <> i.course_id
            OR d.band_id <> i.band_id
            OR d.expected_lesson_count <> i.expected_lesson_count
        )
    )
    BEGIN
        THROW 51002, 'Cannot delete or alter core configuration of a TrainingProgram with existing classes.', 1;
    END;
END;
