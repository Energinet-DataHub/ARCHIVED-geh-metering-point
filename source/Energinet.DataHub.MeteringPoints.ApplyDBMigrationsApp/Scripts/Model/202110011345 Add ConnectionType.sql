ALTER TABLE [dbo].[MarketMeteringPoints]
    ADD
    ConnectionType [NVARCHAR](50) NULL
GO
ALTER TABLE [dbo].[ConsumptionMeteringPoints]
DROP COLUMN ConnectionType;