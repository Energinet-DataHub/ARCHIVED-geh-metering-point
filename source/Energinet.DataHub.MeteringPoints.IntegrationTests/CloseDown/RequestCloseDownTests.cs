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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.CloseDown;
using Energinet.DataHub.MeteringPoints.Application.Common.ReceiveBusinessRequests;
using Energinet.DataHub.MeteringPoints.Application.Common.SystemTime;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using MediatR;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.CloseDown
{
    [IntegrationTest]
    public class RequestCloseDownTests : TestHost
    {
        public RequestCloseDownTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            SetCurrentAuthenticatedActor(SampleData.GridOperatorIdOfGrid870);
        }

        [Fact]
        public async Task Reject_if_business_rules_are_violated()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);
            await CloseDownMeteringPointAsync().ConfigureAwait(false);

            var request = CreateRequest();
            await ReceiveRequest(request).ConfigureAwait(false);

            AssertRejectMessage(DocumentType.RejectCloseDownRequest, "D14");
        }

        [Fact]
        public async Task Confirm_should_contain_correct_business_reason_code()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);

            var request = CreateRequest();
            await ReceiveRequest(request).ConfigureAwait(false);

            await AssertMeteringPointExistsAsync(request.GsrnNumber).ConfigureAwait(false);
            AssertConfirmMessage(DocumentType.AcceptCloseDownRequest, "D14");
        }

        [Fact]
        public async Task Reject_should_contain_correct_business_reason_code()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);
            await CloseDownMeteringPointAsync().ConfigureAwait(false);

            var request = CreateRequest();
            await ReceiveRequest(request).ConfigureAwait(false);

            AssertRejectMessage(DocumentType.RejectCloseDownRequest, "D14");
        }

        [Fact]
        public async Task A_new_process_is_started_when_request_is_received()
        {
            var request = CreateRequest();
            await ReceiveRequest(request).ConfigureAwait(false);

            AssertProcess()
                .IsProcessType(BusinessProcessType.CloseDownMeteringPoint)
                .HasTransactionId(request.TransactionId);
        }

        [Fact]
        public async Task Request_is_accepted_if_validation_check_is_passed()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);

            var request = CreateRequest();
            await ReceiveRequest(request).ConfigureAwait(false);

            AssertProcess()
                .HasStatus("RequestWasAccepted");
            AssertConfirmMessage(DocumentType.AcceptCloseDownRequest, "D14");
        }

        [Fact]
        public async Task Request_is_rejected_if_validation_check_is_fails()
        {
            var request = CreateRequest();
            await ReceiveRequest(request).ConfigureAwait(false);

            AssertProcess()
                .HasStatus("RequestWasRejected");
            AssertRejectMessage(DocumentType.RejectCloseDownRequest, "D14");
        }

        [Fact]
        public async Task Reject_if_gsrn_number_is_missing()
        {
            var request = CreateRequest()
                with
                {
                    GsrnNumber = string.Empty,
                };
            await ReceiveRequest(request).ConfigureAwait(false);

            AssertValidationError("D57");
        }

        [Fact]
        public async Task Reject_if_transaction_id_is_missing()
        {
            var request = CreateRequest()
                with
                {
                    TransactionId = string.Empty,
                };

            await ReceiveRequest(request).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Fact]
        public async Task Reject_if_effective_date_is_invalid()
        {
            var request = CreateRequest()
                with
                {
                    EffectiveDate = string.Empty,
                };
            await ReceiveRequest(request).ConfigureAwait(false);

            AssertValidationError("D02");
        }

        [Fact]
        public async Task Reject_if_metering_point_does_not_exist()
        {
            var request = CreateRequest();

            await ReceiveRequest(request).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Fact]
        public async Task Metering_point_is_closed_down_when_due_date_has_transpired()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);
            var request = CreateRequest();
            await ReceiveRequest(request).ConfigureAwait(false);

            await TimeHasPassed(request.EffectiveDate).ConfigureAwait(false);

            AssertMasterData()
                .HasConnectionState(PhysicalState.ClosedDown);
        }

        private static MasterDataDocument CreateRequest()
        {
            return new MasterDataDocument(
                ProcessType: BusinessProcessType.CloseDownMeteringPoint.Name,
                TransactionId: SampleData.Transaction,
                EffectiveDate: SampleData.EffectiveDate,
                GsrnNumber: SampleData.GsrnNumber);
        }

        private Task TimeHasPassed(string now)
        {
            return GetService<IMediator>()
                .Publish(new TimeHasPassed(InstantPattern.General.Parse(now).Value), CancellationToken.None);
        }

        private IRequestReceiver CreateReceiver()
        {
            return GetService<RequestReceiver>();
        }

        private Task ReceiveRequest(MasterDataDocument request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var receiver = CreateReceiver();
            return receiver.ReceiveRequestAsync(request);
        }
    }
}
