ALTER TABLE [dbo].[MarketMeteringPoints]
    DROP COLUMN StartOfSupplyDate;

CREATE TABLE [dbo].[EnergySupplierDetails]
(
    [Id]                        [uniqueidentifier]   NOT NULL,
    [RecordId]                  [int] IDENTITY (1,1) NOT NULL,
    [StartOfSupplyDate]         [datetime2](7)       NOT NULL,
    [GlnNumber]                 [nvarchar](50)       NOT NULL,
    [MarketMeteringPointId]     [uniqueidentifier]   NOT NULL,

    CONSTRAINT [PK_EnergySupplierDetails] PRIMARY KEY NONCLUSTERED ([Id]),
    CONSTRAINT [FK_EnergySupplierDetails_MarketMeteringPoints] FOREIGN KEY ([MarketMeteringPointId]) REFERENCES [MarketMeteringPoints] ([Id])
)

CREATE UNIQUE CLUSTERED INDEX CIX_EnergySupplierDetails ON EnergySupplierDetails([RecordId])
