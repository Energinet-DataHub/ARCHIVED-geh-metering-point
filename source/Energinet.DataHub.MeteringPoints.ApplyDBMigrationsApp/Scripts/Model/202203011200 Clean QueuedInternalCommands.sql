DELETE FROM [dbo].[QueuedInternalCommands]
WHERE ProcessedDate is null
AND ScheduleDate is null