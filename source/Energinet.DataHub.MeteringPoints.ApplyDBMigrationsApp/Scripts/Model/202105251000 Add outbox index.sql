IF EXISTS (SELECT name FROM sys.indexes
    WHERE name = N'ProcessedOutboxMessagesNonClusteredIndex'
    AND object_id = OBJECT_ID (N'dbo.OutboxMessages'))
DROP INDEX ProcessedOutboxMessagesNonClusteredIndex
    ON OutboxMessages
GO

CREATE NONCLUSTERED INDEX ProcessedOutboxMessagesNonClusteredIndex
    ON dbo.OutboxMessages ([CreationDate] ASC, [Category]) INCLUDE ([Id], [Data], [Type], [ProcessedDate])
    WHERE ProcessedDate IS NULL;
GO