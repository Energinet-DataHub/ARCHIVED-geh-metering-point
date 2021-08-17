ALTER TABLE [dbo].[MeteringPoints] 
    DROP COLUMN PhysicalStatusOfMeteringPoint
GO
ALTER TABLE [dbo].[MeteringPoints]
ADD 
    ConnectionState_PhysicalState NVARCHAR(255) NOT NULL,
    ConnectionState_EffectiveDate DATETIME2(7) NULL;
GO
