ALTER TABLE [dbo].[ExchangeMeteringPoints]
    DROP COLUMN ToGrid

ALTER TABLE [dbo].[ExchangeMeteringPoints]
    DROP COLUMN FromGrid

ALTER TABLE [dbo].[MeteringPoints]
    ADD ToGrid [UNIQUEIDENTIFIER] NULL

ALTER TABLE [dbo].[MeteringPoints]
    ADD FromGrid [UNIQUEIDENTIFIER] NULL