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

using System.Collections.Generic;
using System.Linq;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.Authorization
{
    public class InputAuthorizationHandler<TCommand, TResult> : IAuthorizationHandler<TCommand, TResult>
        where TCommand : IRequest<TResult>
    {
        private readonly List<IAuthorizationHandler<TCommand, TResult>> _validators;

        public InputAuthorizationHandler(List<IAuthorizationHandler<TCommand, TResult>> validators)
        {
            _validators = validators;
        }

        public AuthorizationResult Validate(TCommand command)
        {
            var validationErrors = _validators.SelectMany(x =>
            {
                var validationResult = x.Validate(command);
                return validationResult.Errors;
            }).ToList();

            return new AuthorizationResult(validationErrors);
        }
    }
}
