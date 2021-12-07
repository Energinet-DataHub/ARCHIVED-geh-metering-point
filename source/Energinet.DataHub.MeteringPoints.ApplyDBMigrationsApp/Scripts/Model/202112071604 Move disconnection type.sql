ALTER TABLE [dbo].[MarketMeteringPoints]
    DROP COLUMN DisconnectionType

ALTER TABLE [dbo].[MeteringPoints]
    ADD DisconnectionType [NVARCHAR](10) NULL