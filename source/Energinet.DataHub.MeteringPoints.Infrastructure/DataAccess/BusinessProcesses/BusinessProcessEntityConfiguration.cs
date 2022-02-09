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
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses.CloseDown;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.BusinessProcesses
{
    public class BusinessProcessEntityConfiguration : IEntityTypeConfiguration<BusinessProcess>
    {
        public void Configure(EntityTypeBuilder<BusinessProcess> builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            builder.ToTable("BusinessProcesses", "dbo");

            builder.HasDiscriminator<BusinessProcessType>("ProcessType")
                .HasValue<CloseDownProcess>(BusinessProcessType.CloseDownMeteringPoint);

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasConversion(
                    toDbValue => toDbValue.Value,
                    fromDbValue => BusinessProcessId.Create(fromDbValue));
            builder.Property<string>("TransactionId")
                .HasColumnName("TransactionId");

            builder.Property<BusinessProcessType>("ProcessType")
                .HasColumnName("ProcessType")
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<BusinessProcessType>(fromDbValue));
        }
    }
}
