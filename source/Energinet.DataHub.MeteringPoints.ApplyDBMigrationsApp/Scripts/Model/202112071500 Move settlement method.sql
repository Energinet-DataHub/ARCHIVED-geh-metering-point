ALTER TABLE [dbo].[ConsumptionMeteringPoints]
    DROP COLUMN SettlementMethod

ALTER TABLE [dbo].[MeteringPoints]
    ADD SettlementMethod [NVARCHAR](50) NULL