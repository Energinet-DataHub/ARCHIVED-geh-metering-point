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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Infrastructure.SubPostOffice;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.PostOffice
{
    public class PostOfficeMessageMetadataRepository : IPostOfficeMessageMetadataRepository
    {
        private readonly MeteringPointContext _context;

        public PostOfficeMessageMetadataRepository(MeteringPointContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<PostOfficeMessage> GetMessageAsync(Guid messageId)
        {
            return _context.PostOfficeMessages.SingleAsync(p => p.Id == messageId);
        }

        public Task<PostOfficeMessage[]> GetMessagesAsync(Guid[] messageIds)
        {
            return _context.PostOfficeMessages.Where(p => messageIds.Contains(p.Id)).ToArrayAsync();
        }

        public void AddMessageMetadata(PostOfficeMessage postOfficeMessage)
        {
            _context.PostOfficeMessages.Add(postOfficeMessage);
        }
    }
}
