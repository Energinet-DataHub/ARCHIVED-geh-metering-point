CREATE TABLE [dbo].[MeteringPoints]
(
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [RecordId] INT IDENTITY(1,1) NOT NULL,
    [GsrnNumber] NVARCHAR(36) NOT NULL,
    [StreetName] NVARCHAR(500) NOT NULL,
    [PostCode] NVARCHAR(10) NOT NULL,
    [CityName] NVARCHAR(100) NOT NULL,
    [CountryCode] NVARCHAR(4) NOT NULL,
    [IsAddressWashable] BIT NOT NULL DEFAULT(0),
    [PhysicalStatusOfMeteringPoint] NVARCHAR(255) NOT NULL,
    [MeteringPointSubType] NVARCHAR(50) NULL,
    [MeterReadingOccurrence] NVARCHAR(50) NULL,
    [TypeOfMeteringPoint] NVARCHAR(50) NOT NULL,
    [MaximumCurrent] INT NOT NULL DEFAULT(0),
    [MaximumPower] INT NOT NULL DEFAULT(0),
    [MeteringGridArea] NVARCHAR(255) NULL,
    [PowerPlant] NVARCHAR(255) NULL,
    [LocationDescription] NVARCHAR(255) NULL,
    [ProductType] NVARCHAR(255) NULL,
    [ParentRelatedMeteringPoint] NVARCHAR(255) NULL,
    [UnitType] NVARCHAR(255) NULL,  
    [OccurenceDate] DATETIME2(7) NULL,
    [MeterNumber] NVARCHAR(255) NULL,
    
    CONSTRAINT [PK_MeteringPoints] PRIMARY KEY NONCLUSTERED ([Id])
)
    
CREATE UNIQUE CLUSTERED INDEX CIX_MeteringPoints ON MeteringPoints([RecordId])
CREATE UNIQUE INDEX CIX_MeteringPoints_GsrnNumber ON MeteringPoints([GsrnNumber])

CREATE TABLE [dbo].[ConsumptionMeteringPoints]
(
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [RecordId] INT IDENTITY(1,1) NOT NULL,
    
    [SettlementMethod] NVARCHAR(255) NULL,
    [NetSettlementGroup] NVARCHAR(255) NULL,
    [DisconnectionType] NVARCHAR(255) NULL,
    [ConnectionType] NVARCHAR(255) NULL,
    [AssetType] NVARCHAR(255) NULL,
    
    CONSTRAINT [PK_ConsumptionMeteringPoints] PRIMARY KEY NONCLUSTERED ([Id]),
    CONSTRAINT [FK_ConsumptionMeteringPoints_MeteringPoints_Id] FOREIGN KEY ([Id]) REFERENCES [MeteringPoints] ([Id])
)
    
CREATE UNIQUE CLUSTERED INDEX CIX_ConsumptionMeteringPoints ON dbo.ConsumptionMeteringPoints([RecordId])

CREATE TABLE [dbo].[ProductionMeteringPoints]
(
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [RecordId] INT IDENTITY(1,1) NOT NULL,
    
    [ProductionObligation] NVARCHAR(255) NULL,
    [NetSettlementGroup] NVARCHAR(255) NULL,
    [DisconnectionType] NVARCHAR(255) NULL,
    [ConnectionType] NVARCHAR(255) NULL,

    CONSTRAINT [PK_ProductionMeteringPoints] PRIMARY KEY NONCLUSTERED ([Id]),
    CONSTRAINT [FK_ProductionMeteringPoints_MeteringPoints_Id] FOREIGN KEY ([Id]) REFERENCES [MeteringPoints] ([Id])
)
    
CREATE UNIQUE CLUSTERED INDEX CIX_ProductionMeteringPoints ON dbo.ProductionMeteringPoints([RecordId])

CREATE TABLE [dbo].[ExchangeMeteringPoints]
(
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [RecordId] INT IDENTITY(1,1) NOT NULL,
    [ToGrid] NVARCHAR(255) NULL,
    [FromGrid] NVARCHAR(255) NULL,
    
    CONSTRAINT [PK_ExchangeMeteringPoints] PRIMARY KEY NONCLUSTERED ([Id]),
    CONSTRAINT [FK_ExchangeMeteringPoints_MeteringPoints_Id] FOREIGN KEY ([Id]) REFERENCES [MeteringPoints] ([Id])
)

CREATE UNIQUE CLUSTERED INDEX CIX_ExchangeMeteringPoints ON dbo.ExchangeMeteringPoints([RecordId])