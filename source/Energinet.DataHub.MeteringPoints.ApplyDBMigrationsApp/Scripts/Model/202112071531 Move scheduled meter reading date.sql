ALTER TABLE [dbo].[ConsumptionMeteringPoints]
    DROP COLUMN ScheduledMeterReadingDate

ALTER TABLE [dbo].[MeteringPoints]
    ADD ScheduledMeterReadingDate [NVARCHAR](10) NULL