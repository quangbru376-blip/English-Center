CREATE OR ALTER TRIGGER [dbo].[trg_Band_ThresholdSetFreezeGuard]
ON [dbo].[Band]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Gate check: caller must prehold Exclusive Transaction public EC:BANDSET lock.
    -- The trigger does not acquire the lock; callers must acquire it prior to DML.
    IF APPLOCK_MODE(N'public', N'EC:BANDSET', N'Transaction') IS NULL
       OR APPLOCK_MODE(N'public', N'EC:BANDSET', N'Transaction') <> N'Exclusive'
    BEGIN
        THROW 51021, N'Band threshold set modification requires preheld Exclusive Transaction lock on EC:BANDSET.', 1;
    END;

    -- If no rows were affected, exit early.
    IF NOT EXISTS (SELECT 1 FROM [inserted])
       AND NOT EXISTS (SELECT 1 FROM [deleted])
    BEGIN
        RETURN;
    END;

    -- Invariant check:
    -- After any completed EntryTestAttempt (score IS NOT NULL, completed_booking_id IS NOT NULL),
    -- the threshold set is frozen: reject any insert, delete, or band_code / min_score modification.
    -- First-result callers in future must acquire the same EC:BANDSET lock before DML;
    -- this trigger alone cannot prevent bypass by an unrelated direct first-result writer.
    IF EXISTS (
        SELECT 1
        FROM [dbo].[EntryTestAttempt]
        WHERE [score] IS NOT NULL
          AND [completed_booking_id] IS NOT NULL
    )
    BEGIN
        -- Check for any inserted rows (row in inserted with no matching band_id in deleted)
        IF EXISTS (
            SELECT 1
            FROM [inserted] [i]
            WHERE NOT EXISTS (
                SELECT 1
                FROM [deleted] [d]
                WHERE [d].[band_id] = [i].[band_id]
            )
        )
        -- Check for any deleted rows (row in deleted with no matching band_id in inserted)
        OR EXISTS (
            SELECT 1
            FROM [deleted] [d]
            WHERE NOT EXISTS (
                SELECT 1
                FROM [inserted] [i]
                WHERE [i].[band_id] = [d].[band_id]
            )
        )
        -- Check for any modified band_code or min_score (compare by band_id, not UPDATE(col); exact BIN2 + DATALENGTH)
        OR EXISTS (
            SELECT 1
            FROM [inserted] [i]
            INNER JOIN [deleted] [d]
                ON [d].[band_id] = [i].[band_id]
            WHERE [i].[min_score] <> [d].[min_score]
               OR [i].[band_code] COLLATE Latin1_General_100_BIN2 <> [d].[band_code] COLLATE Latin1_General_100_BIN2
               OR DATALENGTH([i].[band_code]) <> DATALENGTH([d].[band_code])
        )
        BEGIN
            THROW 51022, N'Band threshold set is frozen after entry test attempts have completed.', 1;
        END;
    END;
END;
