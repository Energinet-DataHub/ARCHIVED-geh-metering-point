CREATE TABLE [dbo].[GridCompany]
(
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [RecordId] INT IDENTITY(1,1) NOT NULL,
    [ActorId] nvarchar(50) NOT NULL,
    [ActorType] nvarchar(50) NOT NULL,
    [Roles] nvarchar(max) NOT NULL,

    CONSTRAINT [PK_GridCompany] PRIMARY KEY NONCLUSTERED ([Id])
)
ALTER TABLE [dbo].[GridAreaLinks]
    DROP CONSTRAINT [FK_GridAreaLinks_GridAreas]

TRUNCATE TABLE [dbo].[GridAreaLinks]

TRUNCATE TABLE [dbo].[GridAreas]

ALTER TABLE [dbo].[GridAreaLinks]
    ADD CONSTRAINT [FK_GridAreaLinks_GridAreas]
        FOREIGN KEY ([GridAreaId]) REFERENCES [GridAreas]([Id])

ALTER TABLE [dbo].[GridAreas]
    ADD [GridCompanyId] UNIQUEIDENTIFIER NOT NULL

ALTER TABLE [dbo].[GridAreas]
    ADD CONSTRAINT FK_GridAreas_GridCompany
        FOREIGN KEY (GridCompanyId) REFERENCES [dbo].[GridCompany](Id)