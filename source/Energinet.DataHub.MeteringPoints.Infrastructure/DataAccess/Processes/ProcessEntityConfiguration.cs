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
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.Processes
{
#pragma warning disable SA1402 // All Configurations in this file is related to MeteringPoints
    public class ProcessEntityConfiguration : IEntityTypeConfiguration<Process>
    {
        public void Configure(EntityTypeBuilder<Process> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ToTable("Processes", "dbo");

            builder.HasKey(process => process.Id);

            builder.Property(process => process.EffectiveDate)
                .HasConversion(
                    toDbValue => toDbValue.GetValueOrDefault(),
                    fromDbValue => new DateTime(fromDbValue.Ticks, DateTimeKind.Utc));

            builder.Property(process => process.CreatedDate)
                .HasConversion(
                    toDbValue => toDbValue,
                    fromDbValue => new DateTime(fromDbValue.Ticks, DateTimeKind.Utc));

            builder
                .OwnsMany(process => process.Details, details =>
                {
                    details.ToTable("ProcessDetails")
                        .WithOwner()
                        .HasForeignKey(processDetail => processDetail.ProcessId);

                    details.HasKey(processDetail => processDetail.Id);

                    details.Property(processDetail => processDetail.EffectiveDate)
                        .HasConversion(
                            toDbValue => toDbValue.GetValueOrDefault(),
                            fromDbValue => new DateTime(fromDbValue.Ticks, DateTimeKind.Utc));

                    details.Property(processDetail => processDetail.CreatedDate)
                        .HasConversion(
                            toDbValue => toDbValue,
                            fromDbValue => new DateTime(fromDbValue.Ticks, DateTimeKind.Utc));

                    details.OwnsMany(processDetail => processDetail.Errors, errors =>
                    {
                        errors.ToTable("ProcessDetailErrors")
                            .WithOwner()
                            .HasForeignKey(error => error.ProcessDetailId);

                        errors.HasKey(error => error.Id);
                    });
                });
        }
    }
}
