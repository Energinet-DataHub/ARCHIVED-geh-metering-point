ALTER TABLE [dbo].[MarketMeteringPoints]
    DROP COLUMN NetSettlementGroup

ALTER TABLE [dbo].[MeteringPoints]
    ADD NetSettlementGroup [NVARCHAR](10) NULL