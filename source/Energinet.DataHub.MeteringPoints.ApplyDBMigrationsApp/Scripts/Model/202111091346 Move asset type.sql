ALTER TABLE [dbo].[ConsumptionMeteringPoints]
    DROP COLUMN AssetType

ALTER TABLE [dbo].[MeteringPoints]
    ADD AssetType [NVARCHAR](255) NULL