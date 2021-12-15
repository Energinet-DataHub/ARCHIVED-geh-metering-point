ALTER TABLE [dbo].[ProductionMeteringPoints]
    DROP COLUMN ProductionObligation

ALTER TABLE [dbo].[MeteringPoints]
    ADD ProductionObligation [bit] NULL default 0