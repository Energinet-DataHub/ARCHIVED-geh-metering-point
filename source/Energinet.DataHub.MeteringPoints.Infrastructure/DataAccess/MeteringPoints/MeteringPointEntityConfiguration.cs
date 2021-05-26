// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
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

            builder.Property(x => x.StreetName);
            builder.Property(x => x.PostCode);
            builder.Property(x => x.CityName);
            builder.Property(x => x.CountryCode);
            builder.Property(x => x.IsAddressWashable);

            builder.Property(x => x.PhysicalState)
                .HasColumnName("PhysicalStatusOfMeteringPoint")
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<PhysicalState>(fromDbValue));

            builder.Property(x => x.MeteringPointSubType)
                .HasColumnName("MeteringPointSubType")
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<MeteringPointSubType>(fromDbValue));

            builder.Property(x => x.MeteringPointType)
                .HasColumnName("TypeOfMeteringPoint")
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<MeteringPointType>(fromDbValue));

            builder.Property(x => x.GridAreaId)
                .HasColumnName("MeteringGridArea")
                .HasConversion(
                    toDbValue => toDbValue.Value,
                    fromDbValue => new GridAreaId(fromDbValue));

            builder.Property(x => x.PowerPlant)
                .HasConversion(toDbValue => toDbValue.Value, fromDbValue => GsrnNumber.Create(fromDbValue));
            builder.Property(x => x.LocationDescription);
            builder.Property(x => x.ProductType)
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<ProductType>(fromDbValue));
            builder.Property(x => x.ParentRelatedMeteringPoint);
            builder.Property(x => x.UnitType);
            builder.Property(x => x.MeterNumber);
            builder.Property(x => x.MeterReadingOccurrence)
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<ReadingOccurrence>(fromDbValue));
            builder.Property(x => x.MaximumCurrent);
            builder.Property(x => x.MaximumPower);
            builder.Property(x => x.OccurenceDate);
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

            builder.Property(x => x.AssetType)
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<AssetType>(fromDbValue));
            builder.Property(x => x.ConnectionType);
            builder.Property(x => x.DisconnectionType)
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<DisconnectionType>(fromDbValue));
            builder.Property(x => x.ConnectionType)
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<ConnectionType>(fromDbValue));
            builder.Property(x => x.SettlementMethod)
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<SettlementMethod>(fromDbValue));
            builder.Property(x => x.NetSettlementGroup);
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

            builder.Property(x => x.ProductionObligation);
            builder.Property(x => x.NetSettlementGroup);
            builder.Property(x => x.DisconnectionType)
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<DisconnectionType>(fromDbValue));
            builder.Property(x => x.ConnectionType)
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<ConnectionType>(fromDbValue));
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

            builder.Property(x => x.ToGrid);
            builder.Property(x => x.FromGrid);
        }
    }
}
