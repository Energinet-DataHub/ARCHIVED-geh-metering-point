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

using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.SubPostOffice;
using Xunit.Sdk;

namespace Energinet.DataHub.MeteringPoints.Tests.SubPostOffice
{
    public class DummySubPostOfficeStorageClient : ISubPostOfficeStorageClient
    {
        private PostOfficeMessageEnvelope? _message;

        public Task WriteAsync(PostOfficeMessageEnvelope message)
        {
            _message = message;
            return Task.CompletedTask;
        }

        public Task<string> ReadAsync(string id)
        {
            if (_message is null)
            {
                throw new NullException(_message);
            }

            return new Task<string>(() => _message.Content);
        }
    }
}
