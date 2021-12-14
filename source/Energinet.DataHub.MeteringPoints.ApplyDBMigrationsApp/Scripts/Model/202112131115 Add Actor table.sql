CREATE TABLE [dbo].[Actor]
(
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [RecordId] INT IDENTITY(1,1) NOT NULL,
    [IdentificationNumber] nvarchar(50) NOT NULL,
    [ActorType] nvarchar(50) NOT NULL,
    [Roles] nvarchar(max) NOT NULL,

    CONSTRAINT [PK_Actor] PRIMARY KEY NONCLUSTERED ([Id])
)
ALTER TABLE [dbo].[GridAreaLinks]
    DROP CONSTRAINT [FK_GridAreaLinks_GridAreas]

TRUNCATE TABLE [dbo].[GridAreaLinks]

TRUNCATE TABLE [dbo].[GridAreas]

ALTER TABLE [dbo].[GridAreaLinks]
    ADD CONSTRAINT [FK_GridAreaLinks_GridAreas]
        FOREIGN KEY ([GridAreaId]) REFERENCES [GridAreas]([Id])

ALTER TABLE [dbo].[GridAreas]
    ADD [ActorId] UNIQUEIDENTIFIER NOT NULL

ALTER TABLE [dbo].[GridAreas]
    ADD CONSTRAINT FK_GridAreas_Actor
        FOREIGN KEY (ActorId) REFERENCES [dbo].[Actor](Id)
