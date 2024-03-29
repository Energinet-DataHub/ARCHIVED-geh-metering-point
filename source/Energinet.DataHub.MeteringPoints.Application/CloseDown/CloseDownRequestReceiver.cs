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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Application.Common.ReceiveBusinessRequests;
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses.CloseDown;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentValidation.Results;
using MediatR;
using NodaTime;
using NodaTime.Text;

namespace Energinet.DataHub.MeteringPoints.Application.CloseDown
{
    public class CloseDownRequestReceiver : IRequestReceiver
    {
        private readonly IBusinessProcessRepository _businessProcesses;
        private readonly IActorMessageService _actorMessageService;
        private readonly RequestCloseDownValidator _validator;
        private readonly IMeteringPointRepository _meteringPoints;
        private readonly IMediator _mediator;
        private readonly ICommandScheduler _scheduler;
        private BusinessRulesValidationResult _validationResult = new();
        private CloseDownProcess? _businessProcess;

        public CloseDownRequestReceiver(
            IBusinessProcessRepository businessProcesses,
            IActorMessageService actorMessageService,
            RequestCloseDownValidator validator,
            IMeteringPointRepository meteringPoints,
            IMediator mediator,
            ICommandScheduler scheduler)
        {
            _businessProcesses = businessProcesses ?? throw new ArgumentNullException(nameof(businessProcesses));
            _actorMessageService = actorMessageService ?? throw new ArgumentNullException(nameof(actorMessageService));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _meteringPoints = meteringPoints;
            _mediator = mediator;
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        public async Task ReceiveRequestAsync(MasterDataDocument request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            InitiateProcess(request);

            _validationResult = await ValidateRequestValuesAsync(request).ConfigureAwait(false);
            if (_validationResult.Success == false)
            {
                await RejectRequestAsync(request).ConfigureAwait(false);
                return;
            }

            var targetMeteringPoint = await FindTargetMeteringPointAsync(request.GsrnNumber).ConfigureAwait(false);
            if (targetMeteringPoint == null)
            {
                await RejectRequestAsync(request).ConfigureAwait(false);
                return;
            }

            _validationResult = targetMeteringPoint.CanCloseDown();
            if (_validationResult.Success == false)
            {
                await RejectRequestAsync(request).ConfigureAwait(false);
                return;
            }

            await AcceptRequestAsync(request).ConfigureAwait(false);

            await ScheduleCloseDownAsync(request).ConfigureAwait(false);
        }

        public bool CanHandleRequest(MasterDataDocument request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return request.ProcessType.Equals(BusinessProcessType.CloseDownMeteringPoint.Name, StringComparison.OrdinalIgnoreCase);
        }

        private static ReadOnlyCollection<ValidationError> ExtractValidationErrorsFrom(ValidationResult validationResult)
        {
            return validationResult
                .Errors
                .Select(error => (ValidationError)error.CustomState)
                .ToList()
                .AsReadOnly();
        }

        private Task ScheduleCloseDownAsync(MasterDataDocument request)
        {
            var closeDownCommand = new CloseDownMeteringPoint(request.GsrnNumber);
            return _scheduler.EnqueueAsync(closeDownCommand, InstantPattern.General.Parse(request.EffectiveDate).Value);
        }

        private async Task<BusinessRulesValidationResult> ValidateRequestValuesAsync(MasterDataDocument request)
        {
            var result = await _validator.ValidateAsync(request).ConfigureAwait(false);
            return new BusinessRulesValidationResult(ExtractValidationErrorsFrom(result).ToArray());
        }

        private async Task CreateRejectMessageAsync(MasterDataDocument request)
        {
            await _actorMessageService
                .SendRequestCloseDownRejectedAsync(request.TransactionId, request.GsrnNumber, ConvertValidationErrorsToErrorMessages())
                .ConfigureAwait(false);
        }

        private List<ErrorMessage> ConvertValidationErrorsToErrorMessages()
        {
            return _validationResult.Errors
                .Select(validationError => ErrorMessageFactory.GetErrorMessage(validationError)).ToList();
        }

        private async Task<MeteringPoint?> FindTargetMeteringPointAsync(string gsrnNumber)
        {
            var targetMeteringPoint = await _meteringPoints
                .GetByGsrnNumberAsync(GsrnNumber.Create(gsrnNumber))
                .ConfigureAwait(false);

            if (targetMeteringPoint is null)
            {
                _validationResult.AddErrors(new MeteringPointMustBeKnownValidationError(gsrnNumber));
            }

            return targetMeteringPoint;
        }

        private async Task AcceptRequestAsync(MasterDataDocument request)
        {
            _businessProcess?.AcceptRequest();
            await CreateAcceptMessageAsync(request).ConfigureAwait(false);
        }

        private async Task RejectRequestAsync(MasterDataDocument request)
        {
            await CreateRejectMessageAsync(request).ConfigureAwait(false);
            _businessProcess?.RejectRequest();
        }

        private void InitiateProcess(MasterDataDocument request)
        {
            _businessProcess = CloseDownProcess.Create(BusinessProcessId.Create(), request.TransactionId);
            _businessProcesses.Add(_businessProcess);
        }

        private async Task CreateAcceptMessageAsync(MasterDataDocument request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            await _actorMessageService
                .SendRequestCloseDownAcceptedAsync(request.TransactionId, request.GsrnNumber)
                .ConfigureAwait(false);
        }
    }
}
