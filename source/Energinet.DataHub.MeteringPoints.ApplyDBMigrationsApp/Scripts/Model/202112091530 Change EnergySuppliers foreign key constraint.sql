ALTER TABLE [dbo].[EnergySuppliers]
    DROP CONSTRAINT FK_EnergySupplier_MarketMeteringPoints

ALTER TABLE [dbo].[EnergySuppliers]
    ADD FOREIGN KEY (MarketMeteringPointId) REFERENCES MeteringPoints(Id);