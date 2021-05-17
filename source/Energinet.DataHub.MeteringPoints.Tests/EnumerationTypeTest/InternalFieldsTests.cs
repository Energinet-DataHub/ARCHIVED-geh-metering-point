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

using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Tests.Assets;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.EnumerationTypeTest
{
    [UnitTest]
    public class InternalFieldsTests
    {
        [Fact]
        public void Given_internal_enumeration_When_lookup_by_name_Then_value_is_returned()
        {
            var fields = EnumerationType.GetAll<DocumentTypes>().ToArray();

            Assert.NotEmpty(fields);
            Assert.Contains(DocumentTypes.Lens, fields);
        }
    }
}
