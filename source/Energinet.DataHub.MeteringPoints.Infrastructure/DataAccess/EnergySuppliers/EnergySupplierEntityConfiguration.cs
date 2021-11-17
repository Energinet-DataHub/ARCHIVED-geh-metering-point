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
using Energinet.DataHub.MeteringPoints.Domain.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.EnergySuppliers
{
    public class EnergySupplierEntityConfiguration : IEntityTypeConfiguration<EnergySupplier>
    {
        public void Configure(EntityTypeBuilder<EnergySupplier> builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.ToTable("EnergySuppliers");

            builder.HasKey(supplier => supplier.Id);

            builder.Property(supplier => supplier.Id).ValueGeneratedNever();

            builder.Property(supplier => supplier.MarketMeteringPointId)
                .HasConversion(
                    toDbValue => toDbValue.Value,
                    fromDbValue => new MeteringPointId(fromDbValue));

            builder.Property(supplier => supplier.StartOfSupply)
                .HasColumnName("StartOfSupplyDate");

            builder.Property(supplier => supplier.GlnNumber)
                .HasConversion(
                    toDbValue => toDbValue.Value,
                    fromDbValue => GlnNumber.Create(fromDbValue));
        }
    }
}
