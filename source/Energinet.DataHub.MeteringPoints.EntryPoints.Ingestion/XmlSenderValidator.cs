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
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Actor;
using Energinet.DataHub.Core.XmlConversion.XmlConverter.Abstractions;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion
{
    public class XmlSenderValidator
    {
        private readonly IActorContext _actorContext;

        public XmlSenderValidator(IActorContext actorContext)
        {
            _actorContext = actorContext;
        }

        public (bool IsValid, string ErrorMessage) ValidateSender(XmlHeaderSender? sender)
        {
            if (_actorContext.CurrentActor is null)
                throw new InvalidOperationException("Can't validate message when current actor is not set (null)");

            return sender?.Id != _actorContext.CurrentActor.Identifier
                ? (false, "Identifier applied for sender was not equal to the identifier of the authorized actor.")
                : (true, string.Empty);
        }
    }
}
