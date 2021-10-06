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
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.SubPostOffice
{
    public class BundleCreator : IBundleCreator
    {
        private readonly IMediator _mediator;
        private readonly IJsonSerializer _jsonSerializer;

        public BundleCreator(
            IJsonSerializer jsonSerializer)
        {
            _mediator = new Mediator(new ServiceFactory(type => { return new string('x', 2); }));
            _jsonSerializer = jsonSerializer;
        }

        public async Task<string> CreateBundleAsync(IList<PostOfficeMessage> messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            var messageType = GetMessageType(messages);
            var documentDtos = GetDocumentDtos(messages, messageType);
            var bundleRequest = CreateBundleRequest(documentDtos, messageType);

            var xmlDocument = await _mediator.Send(bundleRequest).ConfigureAwait(false);
            return (string)xmlDocument!;
        }

        private static Type GetMessageType(IList<PostOfficeMessage> messages)
        {
            var messageType = messages.First().Type;
            var type = Type.GetType(messageType) ?? throw new InvalidOperationException("Unknown type");
            return type;
        }

        private static IBundleRequest CreateBundleRequest(List<object>? documentDtos, Type type)
        {
            var bundleInstance = Activator.CreateInstance(type, documentDtos) as IBundleRequest;
            if (bundleInstance is null)
            {
                throw new InvalidOperationException($"Couldn't create bundle for type: {type.Name}");
            }

            return bundleInstance;
        }

        private List<object> GetDocumentDtos(IList<PostOfficeMessage> messages, Type type)
        {
            List<object> dtos = new();
            foreach (var message in messages)
            {
                dtos.Add(_jsonSerializer.Deserialize(message.Content, type));
            }

            return dtos;
        }
    }

    #pragma warning disable
    public class BundleRequest<TDocument> : IRequest<string>, IBundleRequest
    {
        public BundleRequest(List<TDocument> documents)
        {
            Documents = documents;
        }

        public List<TDocument> Documents { get; }
    }

    public interface IBundleRequest
    {
    }

    public class BundleHandler<TDocument> : IRequestHandler<BundleRequest<TDocument>, string>
    {
        private readonly IDocumentSerializer<TDocument> _documentSerializer;

        public Task<string> Handle(BundleRequest<TDocument> request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_documentSerializer.Serialize(request.Documents));
        }
    }

    internal interface IDocumentSerializer<TDocument>
    {
        public string Serialize(TDocument document);
        public string Serialize(List<TDocument> documents);
    }
}
