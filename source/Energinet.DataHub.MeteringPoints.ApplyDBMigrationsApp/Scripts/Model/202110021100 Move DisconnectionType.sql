ALTER TABLE [dbo].[MarketMeteringPoints]
    ADD
    DisconnectionType [NVARCHAR](50) NULL
GO
ALTER TABLE [dbo].[ConsumptionMeteringPoints]
DROP COLUMN DisconnectionType;