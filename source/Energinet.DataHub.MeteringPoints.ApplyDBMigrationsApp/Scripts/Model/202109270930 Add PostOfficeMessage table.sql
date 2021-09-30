CREATE TABLE [dbo].[PostOfficeMessages]
(
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [RecordId] INT IDENTITY(1,1) NOT NULL,
    [Correlation] NVARCHAR(500) NOT NULL,
    [CreatedDate] DATETIME2(7) NULL,

    CONSTRAINT [PK_PostOfficeMessages] PRIMARY KEY NONCLUSTERED ([Id])
)

CREATE UNIQUE CLUSTERED INDEX CIX_PostOfficeMessages ON PostOfficeMessages([RecordId])