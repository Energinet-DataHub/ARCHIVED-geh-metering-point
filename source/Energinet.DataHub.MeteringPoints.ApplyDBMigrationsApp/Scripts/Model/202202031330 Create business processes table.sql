CREATE TABLE [BusinessProcesses]
(
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [RecordId] INT IDENTITY(1,1) NOT NULL,
    [TransactionId] NVARCHAR(36) NOT NULL,
    [ProcessType] NVARCHAR(50) NOT NULL,

    CONSTRAINT [PK_BusinessProcesses] PRIMARY KEY NONCLUSTERED ([Id])
)
GO

CREATE UNIQUE CLUSTERED INDEX CIX_BusinessProcesses ON BusinessProcesses([RecordId])