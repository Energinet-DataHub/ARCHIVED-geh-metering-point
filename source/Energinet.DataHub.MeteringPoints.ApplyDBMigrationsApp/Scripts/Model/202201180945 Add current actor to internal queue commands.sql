ALTER TABLE QueuedInternalCommands
    ADD CurrentActor NVARCHAR(MAX) NULL

GO

UPDATE QueuedInternalCommands
SET CurrentActor = ''

GO

ALTER TABLE QueuedInternalCommands
    ALTER COLUMN CurrentActor NVARCHAR(MAX) NOT NULL