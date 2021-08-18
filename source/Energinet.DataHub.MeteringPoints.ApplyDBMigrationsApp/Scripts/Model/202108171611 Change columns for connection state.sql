ALTER TABLE [dbo].[MeteringPoints] 
    DROP COLUMN PhysicalStatusOfMeteringPoint
GO
ALTER TABLE [dbo].[MeteringPoints]
ADD 
    ConnectionState_PhysicalState NVARCHAR(255) NOT NULL CONSTRAINT DF_MeteringPoints_ConnectionState_PhysicalState DEFAULT 'defaultValue',
    ConnectionState_EffectiveDate DATETIME2(7) NULL;
GO
ALTER TABLE [dbo].[MeteringPoints]
DROP CONSTRAINT DF_MeteringPoints_ConnectionState_PhysicalState
GO
