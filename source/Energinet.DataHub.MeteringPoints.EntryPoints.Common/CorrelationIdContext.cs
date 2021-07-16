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
using System.Threading;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Common
{
    public static class CorrelationIdContext
    {
        private static readonly AsyncLocal<string> _correlationId = new();
        private static readonly AsyncLocal<string> _parentId = new();

        public static string? CorrelationId => _correlationId.Value;

        public static string? ParentId => _parentId.Value;

        public static void SetParentId(string parentId)
        {
            _parentId.Value = parentId;
        }

        public static void SetCorrelationId(string correlationId)
        {
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentException("Correlation Id cannot be null or empty", nameof(correlationId));
            }

            if (!string.IsNullOrWhiteSpace(_correlationId.Value))
            {
                throw new InvalidOperationException("Correlation Id is already set for the context");
            }

            _correlationId.Value = correlationId;
        }
    }
}
