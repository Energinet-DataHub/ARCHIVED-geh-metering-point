﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Exchange;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Production;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints
{
#pragma warning disable SA1402 // All Configurations in this file is related to MeteringPoints
    public class MeteringPointEntityConfiguration : IEntityTypeConfiguration<MeteringPoint>
    {
        public void Configure(EntityTypeBuilder<MeteringPoint> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ToTable("MeteringPoints", "dbo");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasConversion(
                    toDbValue => toDbValue.Value,
                    fromDbValue => new MeteringPointId(fromDbValue));

            builder.Property(x => x.GsrnNumber)
                .HasConversion(
                    toDbValue => toDbValue.Value,
                    fromDbValue => GsrnNumber.Create(fromDbValue));

            builder.OwnsOne<MasterData>("_masterData", mapper =>
            {
                mapper.Property(x => x.ProductType)
                    .HasColumnName("ProductType")
                    .HasConversion(
                        toDbValue => toDbValue.Name,
                        fromDbValue => EnumerationType.FromName<ProductType>(fromDbValue));
                mapper.Property(x => x.UnitType)
                    .HasColumnName("UnitType")
                    .HasConversion(
                        toDbValue => toDbValue.Name,
                        fromDbValue => EnumerationType.FromName<MeasurementUnitType>(fromDbValue));
                mapper.Property(x => x.AssetType)
                    .HasColumnName("AssetType")
                    .HasConversion(
                        toDbValue => toDbValue! == null! ? null : toDbValue.Name,
                        fromDbValue => !string.IsNullOrEmpty(fromDbValue)
                            ? EnumerationType.FromName<AssetType>(fromDbValue)
                            : null!);
                mapper.Property(x => x.ReadingOccurrence)
                    .HasColumnName("MeterReadingOccurrence")
                    .HasConversion(
                        toDbValue => toDbValue.Name,
                        fromDbValue => EnumerationType.FromName<ReadingOccurrence>(fromDbValue));
                mapper.OwnsOne(x => x.PowerLimit, y =>
                {
                    y.Property(x => x.Ampere)
                        .HasColumnName("MaximumCurrent");
                    y.Property(x => x.Kwh)
                        .HasColumnName("MaximumPower");
                });
                mapper.Property(x => x.PowerPlantGsrnNumber)
                    .HasColumnName("PowerPlant")
                    .HasConversion(toDbValue => toDbValue == null ? null : toDbValue.Value, fromDbValue => string.IsNullOrEmpty(fromDbValue) ? null : GsrnNumber.Create(fromDbValue));
                mapper.Ignore(x => x.Address);
                mapper.Ignore(x => x.Capacity);
                mapper.Ignore(x => x.ConnectionType);
                mapper.Ignore(x => x.DisconnectionType);
                mapper.Ignore(x => x.EffectiveDate);
                mapper.Ignore(x => x.MeteringConfiguration);
                mapper.Ignore(x => x.SettlementMethod);
                mapper.Ignore(x => x.NetSettlementGroup);
                mapper.Ignore(x => x.ScheduledMeterReadingDate);
            });

            builder.OwnsOne<Address>("Address", y =>
            {
                y.Property(x => x.StreetName).HasColumnName("StreetName");
                y.Property(x => x.StreetCode).HasColumnName("StreetCode");
                y.Property(x => x.City).HasColumnName("CityName");
                y.Property(x => x.CountryCode)
                    .HasColumnName("CountryCode")
                    .HasConversion(
                        toDbValue => toDbValue! == null! ? null : toDbValue.Name,
                        fromDbValue => !string.IsNullOrWhiteSpace(fromDbValue)
                            ? EnumerationType.FromName<CountryCode>(fromDbValue)
                            : null);
                y.Property(x => x.PostCode).HasColumnName("PostCode");
                y.Property(x => x.CitySubDivision).HasColumnName("CitySubdivision");
                y.Property(x => x.Floor).HasColumnName("Floor");
                y.Property(x => x.Room).HasColumnName("Room");
                y.Property(x => x.BuildingNumber).HasColumnName("BuildingNumber");
                y.Property(x => x.MunicipalityCode).HasColumnName("MunicipalityCode");
                y.Property(x => x.IsActual).HasColumnName("IsActualAddress");
                y.Property(x => x.GeoInfoReference).HasColumnName("GeoInfoReference");
                y.Property(x => x.LocationDescription).HasColumnName("LocationDescription");
            });

            builder.OwnsOne<ConnectionState>("ConnectionState", config =>
            {
                config.Property(x => x.PhysicalState)
                    .HasColumnName("ConnectionState_PhysicalState")
                    .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<PhysicalState>(fromDbValue));
                config.Property(x => x.EffectiveDate)
                    .HasColumnName("ConnectionState_EffectiveDate");
            });

            builder.Property<MeteringPointType>("_meteringPointType")
                .HasColumnName("TypeOfMeteringPoint")
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<MeteringPointType>(fromDbValue));

            builder.Property<GridAreaLinkId>("_gridAreaLinkId")
                .HasColumnName("MeteringGridArea")
                .HasConversion(
                    toDbValue => toDbValue.Value,
                    fromDbValue => new GridAreaLinkId(fromDbValue));

            builder.OwnsOne<MeteringConfiguration>("MeteringConfiguration", mapper =>
            {
                mapper.Property(x => x.Meter)
                    .HasColumnName("MeterNumber")
                    .HasConversion(toDbValue => toDbValue.Value, fromDbValue => MeterId.Create(fromDbValue));
                mapper.Property(x => x.Method)
                    .HasColumnName("MeteringPointSubType")
                    .HasConversion(
                        toDbValue => toDbValue.Name,
                        fromDbValue => EnumerationType.FromName<MeteringMethod>(fromDbValue));
            });

            builder.Property<EffectiveDate>("_effectiveDate")
                .HasColumnName("EffectiveDate")
                .HasConversion<DateTime>(toDbValue => toDbValue.DateInUtc.ToDateTimeUtc(), fromDbValue => EffectiveDate.Create(fromDbValue));

            builder.Property<Capacity>("_capacity")
                .HasColumnName("Capacity")
                .HasConversion<double?>(
                    toDbValue => toDbValue == null
                    ? null
                    : toDbValue.Kw!,
                    convertFromProviderExpression: fromDbValue => fromDbValue.HasValue
                        ? Capacity.Create(fromDbValue.Value)
                        : null!);
        }
    }

    public class MarketMeteringPointEntityConfiguration : IEntityTypeConfiguration<MarketMeteringPoint>
    {
        public void Configure(EntityTypeBuilder<MarketMeteringPoint> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ToTable("MarketMeteringPoints", "dbo");

            builder.OwnsOne<EnergySupplierDetails>("EnergySupplierDetails", config =>
            {
                config.Property(x => x.StartOfSupply)
                    .HasColumnName("StartOfSupplyDate");
            });
            builder.Property<ConnectionType>("ConnectionType")
                .HasColumnName("ConnectionType")
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<ConnectionType>(fromDbValue));

            builder.Property<DisconnectionType>("DisconnectionType")
                .HasColumnName("DisconnectionType")
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<DisconnectionType>(fromDbValue));

            builder.Property<NetSettlementGroup>("_netSettlementGroup")
                .HasColumnName("NetSettlementGroup")
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<NetSettlementGroup>(fromDbValue));
        }
    }

    public class ConsumptionMeteringPointEntityConfiguration : IEntityTypeConfiguration<ConsumptionMeteringPoint>
    {
        public void Configure(EntityTypeBuilder<ConsumptionMeteringPoint> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ToTable("ConsumptionMeteringPoints", "dbo");

            builder.Property<SettlementMethod>("_settlementMethod")
                .HasColumnName("SettlementMethod")
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<SettlementMethod>(fromDbValue));

            builder.Property<ScheduledMeterReadingDate>("_scheduledMeterReadingDate")
                .HasColumnName("ScheduledMeterReadingDate")
                .HasConversion(
                    toDbValue => toDbValue.MonthAndDay,
                    fromDbValue => ScheduledMeterReadingDate.Create(fromDbValue));
        }
    }

    public class ProductionMeteringPointEntityConfiguration : IEntityTypeConfiguration<ProductionMeteringPoint>
    {
        public void Configure(EntityTypeBuilder<ProductionMeteringPoint> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ToTable("ProductionMeteringPoints", "dbo");

            builder.Property("_productionObligation")
                .HasColumnName("ProductionObligation");
        }
    }

    public class ExchangeMeteringPointEntityConfiguration : IEntityTypeConfiguration<ExchangeMeteringPoint>
    {
        public void Configure(EntityTypeBuilder<ExchangeMeteringPoint> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ToTable("ExchangeMeteringPoints", "dbo");

            builder.Property<GridAreaLinkId>("_fromGrid")
                .HasColumnName("FromGrid")
                .HasConversion(toDbValue => toDbValue.Value, fromDbValue => new GridAreaLinkId(fromDbValue));

            builder.Property<GridAreaLinkId>("_toGrid")
                .HasColumnName("ToGrid")
                .HasConversion(toDbValue => toDbValue.Value, fromDbValue => new GridAreaLinkId(fromDbValue));
        }
    }
}
