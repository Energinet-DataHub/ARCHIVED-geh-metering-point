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
using Dapper;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests
{
    public class AssertBusinessProcess
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly dynamic _process;

        private AssertBusinessProcess(dynamic process, IDbConnectionFactory connectionFactory)
        {
            _process = process;
            _connectionFactory = connectionFactory;
        }

        public static AssertBusinessProcess Initialize(string transactionId, IDbConnectionFactory connectionFactory)
        {
            if (connectionFactory == null) throw new ArgumentNullException(nameof(connectionFactory));
            var process = connectionFactory
                .GetOpenConnection()
                .QuerySingleOrDefault(
                $"SELECT * FROM [dbo].[BusinessProcesses] WHERE TransactionId = '{transactionId}'");

            Assert.NotNull(process);
            return new AssertBusinessProcess(process, connectionFactory);
        }

        public AssertBusinessProcess HasTransactionId(string transactionId)
        {
            Assert.Equal(transactionId, _process.TransactionId);
            return this;
        }

        public AssertBusinessProcess IsProcessType(BusinessProcessType businessProcessType)
        {
            Assert.Equal(businessProcessType, EnumerationType.FromName<BusinessProcessType>(_process.ProcessType));
            return this;
        }

        public AssertBusinessProcess HasStatus(string status)
        {
            Assert.Equal(status, _process.Status);
            return this;
        }
    }
}
