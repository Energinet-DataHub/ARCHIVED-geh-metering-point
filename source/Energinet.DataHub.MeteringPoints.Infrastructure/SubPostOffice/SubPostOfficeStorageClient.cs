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

namespace Energinet.DataHub.MeteringPoints.Infrastructure.SubPostOffice
{
    public class SubPostOfficeStorageClient : ISubPostOfficeStorageClient
    {
        private const string ContainerName = "sub-post-office";
        private readonly SubPostOfficeStorageSettings _settings;

        public SubPostOfficeStorageClient(SubPostOfficeStorageSettings settings)
        {
            _settings = settings;
        }

        public Task WriteAsync(PostOfficeMessageBlob messageBlob)
        {
            if (messageBlob == null) throw new ArgumentNullException(nameof(messageBlob));

            var blobContainerClient = new BlobContainerClient(_settings.ConnectionString, ContainerName);

            var blobClient = blobContainerClient.GetBlobClient(messageBlob.BlobName);

            return blobClient.UploadAsync(new BinaryData(messageBlob.Content));
        }

        public async Task<PostOfficeMessageBlob> ReadAsync(string blobName)
        {
            var blobClient = new BlobClient(_settings.ConnectionString, ContainerName, blobName);

            var message = await blobClient.DownloadContentAsync().ConfigureAwait(false);

            return PostOfficeMessageBlobFactory.Create(blobName, message.Value.Content.ToString());
        }
    }

    public record SubPostOfficeStorageSettings(string ConnectionString);
}
