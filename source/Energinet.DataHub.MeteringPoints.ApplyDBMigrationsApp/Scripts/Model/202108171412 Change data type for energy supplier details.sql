ALTER TABLE [dbo].[MarketMeteringPoints] DROP CONSTRAINT FK_MarketMeteringPoints_MeteringPoints_Id
GO
ALTER TABLE [dbo].[ConsumptionMeteringPoints] DROP CONSTRAINT FK_ConsumptionMeteringPoints_MarketMeteringPoints_Id
GO 
DROP TABLE [dbo].[MarketMeteringPoints]
GO
CREATE TABLE [dbo].[MarketMeteringPoints]
(
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [RecordId] INT IDENTITY(1,1) NOT NULL,
    [StartOfSupplyDate] DATETIME2(7) NULL,

    CONSTRAINT [PK_MarketMeteringPoints] PRIMARY KEY NONCLUSTERED ([Id]),
    CONSTRAINT [FK_MarketMeteringPoints_MeteringPoints_Id] FOREIGN KEY ([Id]) REFERENCES [MeteringPoints] ([Id])
    )
GO
ALTER TABLE [dbo].[MarketMeteringPoints] 
   ADD CONSTRAINT [FK_ConsumptionMeteringPoints_MarketMeteringPoints_Id] FOREIGN KEY ([Id]) REFERENCES [MarketMeteringPoints] ([Id])
GO