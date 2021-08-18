EXEC sp_rename '.MeteringPoints.PhysicalStatusOfMeteringPoint', 'ConnectionState_PhysicalState', 'COLUMN';
GO
ALTER TABLE [dbo].[MeteringPoints]
    ADD
    ConnectionState_EffectiveDate DATETIME2(7) NULL;
GO
