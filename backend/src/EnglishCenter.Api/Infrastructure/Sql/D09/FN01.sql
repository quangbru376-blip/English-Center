CREATE OR ALTER FUNCTION dbo.ufn_EffectiveEntryTestState
(
    @status varchar(16),
    @startAt datetime2(3),
    @asOfUtc datetime2(3)
)
RETURNS varchar(16)
WITH SCHEMABINDING
AS
BEGIN
    RETURN (
        CASE
            WHEN @status = 'CANCELLED' THEN 'CANCELLED'
            WHEN @asOfUtc >= @startAt THEN 'CLOSED'
            ELSE @status
        END
    );
END;
