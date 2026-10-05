CREATE OR ALTER VIEW [dbo].[vw_CurrentScheduleOccurrences]
AS
SELECT
    s.[schedule_id] AS [scheduleId],
    s.[class_id] AS [classId],
    c.[class_code] AS [classCode],
    c.[status] AS [classStatus],
    c.[teacher_id] AS [teacherId],
    s.[session_type] AS [type],
    s.[obligation_key] AS [obligationKey],
    s.[session_date] AS [date],
    s.[start_time] AS [startTime],
    s.[end_time] AS [endTime],
    s.[room] AS [room],
    s.[occurrence_state] AS [occurrenceState],
    s.[replaces_schedule_id] AS [replacesScheduleId],
    s.[row_version] AS [version]
FROM [dbo].[Schedule] AS s
INNER JOIN [dbo].[Class] AS c
    ON s.[class_id] = c.[class_id]
WHERE s.[is_current] = 1
  AND c.[status] <> 'CANCELLED';
