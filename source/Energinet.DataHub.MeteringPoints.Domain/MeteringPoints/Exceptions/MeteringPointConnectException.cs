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
using System.Runtime.Serialization;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Exceptions
{
    public class MeteringPointConnectException : BusinessOperationException
    {
        public MeteringPointConnectException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        public MeteringPointConnectException()
        {
        }

        public MeteringPointConnectException(string? message)
            : base(message)
        {
        }

        protected MeteringPointConnectException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public static MeteringPointConnectException Create(MeteringPointId meteringPointId, GsrnNumber gsrnNumber)
        {
            if (meteringPointId == null) throw new ArgumentNullException(nameof(meteringPointId));
            if (gsrnNumber == null) throw new ArgumentNullException(nameof(gsrnNumber));
            var message = $"Connect of metering point id {meteringPointId.Value} ({gsrnNumber.Value}) failed due to violation of one or more business rules.";
            return new MeteringPointConnectException(message);
        }
    }
}
