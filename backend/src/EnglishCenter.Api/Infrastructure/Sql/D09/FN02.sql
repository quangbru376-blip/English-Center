CREATE OR ALTER FUNCTION dbo.ufn_BandForScore
(
    @score decimal(5,2)
)
RETURNS TABLE
AS
RETURN
(
    SELECT TOP (1)
        b.band_id AS bandId,
        b.min_score AS minScore
    FROM dbo.Band AS b
    WHERE b.min_score <= @score
      AND @score >= 0.00
      AND @score <= 100.00
    ORDER BY b.min_score DESC
);
