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
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;

namespace Energinet.DataHub.MeteringPoints.Application.CloseDown
{
    public class RequestReceiver : IRequestReceiver
    {
        private readonly IEnumerable<IRequestReceiver> _receivers;

        public RequestReceiver(IEnumerable<IRequestReceiver> receivers)
        {
            _receivers = receivers;
        }

        public Task ReceiveRequestAsync(MasterDataDocument request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var receiver = _receivers.FirstOrDefault(receiver => receiver.CanHandleRequest(request));
            if (receiver is null)
            {
                throw new NoBusinessRequestReceiverFoundException($"Could not find receiver capable of handling this type {request.ProcessType} of business request.");
            }

            return receiver.ReceiveRequestAsync(request);
        }

        public bool CanHandleRequest(MasterDataDocument request)
        {
            return _receivers.Any(receiver => receiver.CanHandleRequest(request));
        }
    }
}
