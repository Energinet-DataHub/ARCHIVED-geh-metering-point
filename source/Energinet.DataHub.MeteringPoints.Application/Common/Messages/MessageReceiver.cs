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

namespace Energinet.DataHub.MeteringPoints.Application.Common.Messages
{
    public abstract class MessageReceiver<TMessage> : IMessageReceiver<TMessage>
    {
        private readonly IMessageReceiver<TMessage> _next;

        protected MessageReceiver(IMessageReceiver<TMessage> next)
        {
            _next = next;
        }

        public Task HandleAsync(TMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (ShouldHandle(message))
            {
                return ProcessAsync(message);
            }
            else
            {
                return _next.HandleAsync(message);
            }
        }

        protected abstract bool ShouldHandle(TMessage message);

        protected abstract Task ProcessAsync(TMessage message);
    }
}
