ALTER TABLE [dbo].[MarketMeteringPoints]
    ADD
        NetSettlementGroup [NVARCHAR](255) NULL

ALTER TABLE [dbo].[ProductionMeteringPoints]
    DROP COLUMN NetSettlementGroup

ALTER TABLE [dbo].[ConsumptionMeteringPoints]
    DROP COLUMN NetSettlementGroup