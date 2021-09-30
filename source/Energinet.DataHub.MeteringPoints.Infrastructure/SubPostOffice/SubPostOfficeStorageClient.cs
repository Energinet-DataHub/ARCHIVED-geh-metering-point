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
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.SubPostOffice
{
    public class SubPostOfficeStorageClient : ISubPostOfficeStorageClient
    {
        private readonly PostOfficeStorageSettings _settings;

        public SubPostOfficeStorageClient(PostOfficeStorageSettings settings)
        {
            _settings = settings;
        }

        public Task WriteAsync(PostOfficeMessageEnvelope message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var blobContainerClient = new BlobContainerClient(_settings.ConnectionString, "postofficemessages"); // TODO

            var blobClient = blobContainerClient.GetBlobClient("blobName"); // TODO

            return blobClient.UploadAsync(new BinaryData(message.Content));
        }

        public async Task<string> ReadAsync(string id)
        {
            var blobClient = new BlobClient(_settings.ConnectionString, "postofficemessages", id); // TODO

            var message = await blobClient.DownloadContentAsync().ConfigureAwait(false);

            return message.Value.Content.ToString();
        }
    }

    public record PostOfficeStorageSettings(string ConnectionString, string ShareName);
}
