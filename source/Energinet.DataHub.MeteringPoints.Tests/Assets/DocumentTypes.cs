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

using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Tests.Assets
{
    public sealed class DocumentTypes : EnumerationType
    {
        public static readonly DocumentTypes Word = new(1, nameof(Word));
        public static readonly DocumentTypes Excel = new(2, nameof(Excel));
        public static readonly DocumentTypes PowerPoint = new(3, nameof(PowerPoint));
        public static readonly DocumentTypes OneNote = new(4, nameof(OneNote));
        public static readonly DocumentTypes Lens = new(5, nameof(Lens));

        private DocumentTypes(int id, string name)
            : base(id, name)
        {
        }
    }
}
