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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.WebApi.MeteringPoints.Queries
{
    public class MeteringPointProcessesByGsrnQueryHandler : IRequestHandler<MeteringPointProcessesByGsrnQuery, List<Process>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IUserContext _userContext;

        public MeteringPointProcessesByGsrnQueryHandler(IDbConnectionFactory connectionFactory, IUserContext userContext)
        {
            _connectionFactory = connectionFactory;
            _userContext = userContext;
        }

        public Task<List<Process>> Handle(MeteringPointProcessesByGsrnQuery request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return Task.FromResult(new List<Process>
            {
                new Process(
                    Name: "BRS-006",
                    CreatedDate: new DateTime(2022, 2, 1),
                    EffectiveDate: new DateTime(2022, 2, 2),
                    Status: ProcessStatus.Completed,
                    Details: new ProcessDetail[]
                    {
                        new ProcessDetail(
                            Name: "RSM-021",
                            Type: "E58",
                            CreatedDate: new DateTime(2022, 2, 1),
                            EffectiveDate: new DateTime(2022, 2, 2),
                            Receiver: "57000000001",
                            Sender: "DataHub",
                            Status: ProcessStatus.Completed,
                            Errors: Array.Empty<ErrorDto>()),
                    }),
            });
        }

        private static TEnum ConvertEnumerationTypeToEnum<TEnum, TEnumerationType>(string enumerationTypeName)
            where TEnum : Enum
            where TEnumerationType : EnumerationType
        {
            var meteringMethodValueObject = EnumerationType.FromName<TEnumerationType>(enumerationTypeName);

            return (TEnum)(object)meteringMethodValueObject.Id;
        }

        private static TEnum? ConvertNullableEnumerationTypeToEnum<TEnum, TEnumerationType>(string? enumerationTypeName)
            where TEnum : struct, Enum
            where TEnumerationType : EnumerationType
        {
            if (string.IsNullOrEmpty(enumerationTypeName)) return null;
            return ConvertEnumerationTypeToEnum<TEnum, TEnumerationType>(enumerationTypeName);
        }
    }
}
