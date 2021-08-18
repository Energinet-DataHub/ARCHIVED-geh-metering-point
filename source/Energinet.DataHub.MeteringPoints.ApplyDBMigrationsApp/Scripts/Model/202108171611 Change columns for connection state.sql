ALTER TABLE [dbo].[MeteringPoints] 
    DROP COLUMN PhysicalStatusOfMeteringPoint
GO
ALTER TABLE [dbo].[MeteringPoints]
ADD
    ConnectionState_PhysicalState NVARCHAR(255) NULL,
    ConnectionState_EffectiveDate DATETIME2(7) NULL;
GO

UPDATE [dbo].[MeteringPoints]
SET ConnectionState_PhysicalState = 'D03'

ALTER TABLE [dbo].[MeteringPoints]
ALTER COLUMN ConnectionState_PhysicalState NVARCHAR(255) NOT NULL;
GO
