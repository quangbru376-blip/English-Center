CREATE OR ALTER TRIGGER [dbo].[trg_Component_UsedConfigurationGuard]
ON [dbo].[ScoreComponentConfig]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
        RETURN;

    -- Gate check: Caller must prehold Exclusive Transaction public lock on EC:PROGRAM:<id>
    IF EXISTS (
        SELECT 1
        FROM (
            SELECT program_id FROM inserted
            UNION
            SELECT program_id FROM deleted
        ) AS affected(program_id)
        WHERE ISNULL(APPLOCK_MODE('public', N'EC:PROGRAM:' + CAST(affected.program_id AS nvarchar(30)), 'Transaction'), '') <> 'Exclusive'
    )
    BEGIN
        THROW 51011, 'Exclusive transaction lock EC:PROGRAM:<id> is required.', 1;
    END;

    -- Invariant check: Configuration of used programs (any class exists, including cancelled) cannot be modified
    IF EXISTS (
        -- Insert into used program
        SELECT 1
        FROM inserted AS i
        WHERE NOT EXISTS (
            SELECT 1 FROM deleted AS d WHERE d.component_id = i.component_id
        )
        AND EXISTS (
            SELECT 1 FROM [dbo].[Class] AS c WHERE c.program_id = i.program_id
        )

        UNION ALL

        -- Delete from used program
        SELECT 1
        FROM deleted AS d
        WHERE NOT EXISTS (
            SELECT 1 FROM inserted AS i WHERE i.component_id = d.component_id
        )
        AND EXISTS (
            SELECT 1 FROM [dbo].[Class] AS c WHERE c.program_id = d.program_id
        )

        UNION ALL

        -- Update modifying component_name, weight, kind, or program_id where either old or new program is used
        SELECT 1
        FROM inserted AS i
        INNER JOIN deleted AS d ON d.component_id = i.component_id
        WHERE (
            EXISTS (SELECT 1 FROM [dbo].[Class] AS c WHERE c.program_id = d.program_id)
            OR EXISTS (SELECT 1 FROM [dbo].[Class] AS c WHERE c.program_id = i.program_id)
        )
        AND (
            i.program_id <> d.program_id
            OR i.weight <> d.weight
            OR i.kind COLLATE Latin1_General_100_BIN2 <> d.kind COLLATE Latin1_General_100_BIN2
            OR DATALENGTH(i.kind) <> DATALENGTH(d.kind)
            OR i.component_name COLLATE Latin1_General_100_BIN2 <> d.component_name COLLATE Latin1_General_100_BIN2
            OR DATALENGTH(i.component_name) <> DATALENGTH(d.component_name)
        )
    )
    BEGIN
        THROW 51012, 'Score component configuration cannot be modified for a training program with existing classes.', 1;
    END;
END;
