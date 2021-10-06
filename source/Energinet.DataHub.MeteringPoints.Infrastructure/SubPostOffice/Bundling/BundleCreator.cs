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
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.SubPostOffice.Bundling
{
    public class BundleCreator : IBundleCreator
    {
        private readonly IMediator _mediator;

        public BundleCreator(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<string> CreateBundleAsync(IList<PostOfficeMessage> messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            var messageType = GetMessageType(messages);
            var bundleRequest = CreateBundleRequest(messages, messageType);

            var xmlDocument = await _mediator.Send(bundleRequest).ConfigureAwait(false);
            return (string)xmlDocument!;
        }

        private static Type GetMessageType(IList<PostOfficeMessage> messages)
        {
            var messageType = messages.First().Type;
            var type = Type.GetType(messageType) ?? throw new InvalidOperationException("Unknown type");
            return type;
        }

        private static IBundleRequest CreateBundleRequest(IList<PostOfficeMessage> messages, Type type)
        {
            var genericBundleRequestType = typeof(BundleRequest<>).MakeGenericType(type);
            var bundleInstance = Activator.CreateInstance(genericBundleRequestType, messages) as IBundleRequest;
            if (bundleInstance is null)
            {
                throw new InvalidOperationException($"Couldn't create bundle request for type: {type.Name}");
            }

            return bundleInstance;
        }
    }
}
