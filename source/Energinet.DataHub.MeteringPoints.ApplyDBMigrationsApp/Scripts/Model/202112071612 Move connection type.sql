ALTER TABLE [dbo].[MarketMeteringPoints]
    DROP COLUMN ConnectionType

ALTER TABLE [dbo].[MeteringPoints]
    ADD ConnectionType [NVARCHAR](50) NULL