ALTER TABLE QueuedInternalCommands
DROP COLUMN CorrelationId

ALTER TABLE QueuedInternalCommands
    ADD CorrelationId [nvarchar](255) NOT NULL DEFAULT('None');