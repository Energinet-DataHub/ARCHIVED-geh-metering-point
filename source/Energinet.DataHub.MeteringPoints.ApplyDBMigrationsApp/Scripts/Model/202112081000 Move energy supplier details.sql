ALTER TABLE [dbo].[MarketMeteringPoints]
    DROP COLUMN StartOfSupplyDate

ALTER TABLE [dbo].[MeteringPoints]
    ADD StartOfSupplyDate [datetime2](7) NULL