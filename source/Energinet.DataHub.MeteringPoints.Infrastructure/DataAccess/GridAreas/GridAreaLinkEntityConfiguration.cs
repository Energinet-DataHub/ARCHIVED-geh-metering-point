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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.GridAreas
{
#pragma warning disable SA1402 // All Configurations in this file is related to MeteringPoints
    public class GridAreaLinkEntityConfiguration : IEntityTypeConfiguration<GridAreaLink>
    {
        public void Configure(EntityTypeBuilder<GridAreaLink> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ToTable("GridAreaLinks", "dbo");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasConversion(
                    toDbValue => toDbValue.Value,
                    fromDbValue => new GridAreaLinkId(fromDbValue));

            builder.Property<GridAreaId>("_gridAreaId")
                .HasColumnName("GridAreaId")
                .HasConversion(
                    toDbValue => toDbValue.Value,
                    fromDbValue => new GridAreaId(fromDbValue));

            builder.HasOne<GridArea>()
                .WithMany()
                .HasForeignKey("_gridAreaId");
        }
    }
}
