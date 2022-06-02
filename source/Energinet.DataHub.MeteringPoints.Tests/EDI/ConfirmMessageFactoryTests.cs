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
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using Energinet.DataHub.MeteringPoints.Tests.Domain;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests.EDI
{
    public class ConfirmMessageFactoryTests : TestBase
    {
        private MarketRoleParticipant? _sender;
        private MarketRoleParticipant? _receiver;
        private MarketActivityRecord? _marketActivityRecord;
        private DateTime _now;

        [Fact]
        public void Create_metering_point_has_correct_business_reason_code()
        {
            SetupVariables();
            var sut = ConfirmMessageFactory.CreateMeteringPoint(
                _sender!,
                _receiver!,
                Instant.FromUtc(_now.Year, _now.Month, _now.Day, _now.Hour, _now.Minute),
                _marketActivityRecord!);

            Assert.Equal("E02", sut.ProcessType);
        }

        [Fact]
        public void Update_metering_point_has_correct_business_reason_code()
        {
            SetupVariables();
            var sut = ConfirmMessageFactory.UpdateMeteringPoint(
                _sender!,
                _receiver!,
                Instant.FromUtc(_now.Year, _now.Month, _now.Day, _now.Hour, _now.Minute),
                _marketActivityRecord!);

            Assert.Equal("E32", sut.ProcessType);
        }

        [Fact]
        public void Connect_metering_point_has_correct_business_reason_code()
        {
            SetupVariables();
            var sut = ConfirmMessageFactory.ConnectMeteringPoint(
                _sender!,
                _receiver!,
                Instant.FromUtc(_now.Year, _now.Month, _now.Day, _now.Hour, _now.Minute),
                _marketActivityRecord!);

            Assert.Equal("D15", sut.ProcessType);
        }

        [Fact]
        public void Connect_status_metering_point_has_correct_business_reason_code()
        {
            SetupVariables();
            var sut = ConfirmMessageFactory.ConnectionStatusMeteringPoint(
                _sender!,
                _receiver!,
                Instant.FromUtc(_now.Year, _now.Month, _now.Day, _now.Hour, _now.Minute),
                _marketActivityRecord!);

            Assert.Equal("E79", sut.ProcessType);
        }

        [Fact]
        public void Request_close_down_has_correct_business_reason_code()
        {
            SetupVariables();
            var sut = ConfirmMessageFactory.RequestCloseDown(
                _sender!,
                _receiver!,
                Instant.FromUtc(_now.Year, _now.Month, _now.Day, _now.Hour, _now.Minute),
                _marketActivityRecord!);

            Assert.Equal("D14", sut.ProcessType);
        }

        private void SetupVariables()
        {
            _now = DateTime.Now;
            _sender = new MarketRoleParticipant(Guid.NewGuid().ToString(), "A10", "sender");
            _receiver = new MarketRoleParticipant(Guid.NewGuid().ToString(), "A10", "receiver");
            _marketActivityRecord = new MarketActivityRecord(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }
    }
}
