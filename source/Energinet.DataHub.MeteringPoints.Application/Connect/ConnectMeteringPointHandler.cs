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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Authorization;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Extensions;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.Policies;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Connect
{
    public class ConnectMeteringPointHandler : IBusinessRequestHandler<ConnectMeteringPointRequest>
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly MeteringPointPipelineContext _pipelineContext;
        private readonly ISystemDateTimeProvider _systemDateTimeProvider;
        private readonly ConnectSettings _settings;
        private readonly IAuthorizer _authorizer;

        public ConnectMeteringPointHandler(
            IMeteringPointRepository meteringPointRepository,
            MeteringPointPipelineContext pipelineContext,
            ISystemDateTimeProvider systemDateTimeProvider,
            ConnectSettings settings,
            IAuthorizer authorizer)
        {
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
            _pipelineContext = pipelineContext;
            _systemDateTimeProvider = systemDateTimeProvider ?? throw new ArgumentNullException(nameof(systemDateTimeProvider));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _authorizer = authorizer ?? throw new ArgumentNullException(nameof(authorizer));
        }

        public async Task<BusinessProcessResult> Handle(
            ConnectMeteringPointRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var meteringPoint = await FetchTargetMeteringPointAsync(request).ConfigureAwait(false);
            if (meteringPoint == null)
            {
                return new BusinessProcessResult(request.TransactionId, new List<ValidationError>()
                {
                    new MeteringPointMustBeKnownValidationError(request.GsrnNumber),
                });
            }

            var authorizationResult = await _authorizer.AuthorizeAsync(meteringPoint).ConfigureAwait(false);
            if (authorizationResult.Success == false)
            {
                return new BusinessProcessResult(request.TransactionId, authorizationResult.Errors);
            }

            var policyCheckResult = await CheckPoliciesAsync(request).ConfigureAwait(false);
            if (policyCheckResult.Success == false)
            {
                return new BusinessProcessResult(request.TransactionId, policyCheckResult.Errors);
            }

            var connectionDetails = ConnectionDetails.Create(request.EffectiveDate.ToInstant());
            var rulesCheckResult = CheckBusinessRules(request, connectionDetails, meteringPoint!);
            if (!rulesCheckResult.Success)
            {
                return rulesCheckResult;
            }

            meteringPoint.Connect(ConnectionDetails.Create(request.EffectiveDate.ToInstant()));

            _pipelineContext.MeteringPointId = meteringPoint.Id.Value.ToString()!;

            return BusinessProcessResult.Ok(request.TransactionId);
        }

        private static BusinessProcessResult CheckBusinessRules(
            ConnectMeteringPointRequest request,
            ConnectionDetails connectionDetails,
            MeteringPoint meteringPoint)
        {
            var validationResult = meteringPoint.ConnectAcceptable(connectionDetails);

            return new BusinessProcessResult(request.TransactionId, validationResult.Errors);
        }

        private async Task<MeteringPoint?> FetchTargetMeteringPointAsync(ConnectMeteringPointRequest request)
        {
            return await _meteringPointRepository
                .GetByGsrnNumberAsync(GsrnNumber.Create(request.GsrnNumber))
                .ConfigureAwait(false);
        }

        private Task<BusinessRulesValidationResult> CheckPoliciesAsync(ConnectMeteringPointRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var validationResults = new List<BusinessRulesValidationResult>()
            {
                new EffectiveDatePolicy(_settings.NumberOfDaysEffectiveDateIsAllowedToBeforeToday).Check(_systemDateTimeProvider.Now(), EffectiveDate.Create(request.EffectiveDate)),
            };

            var validationErrors = validationResults.SelectMany(results => results.Errors).ToList();
            return Task.FromResult<BusinessRulesValidationResult>(new BusinessRulesValidationResult(validationErrors));
        }
    }
}
