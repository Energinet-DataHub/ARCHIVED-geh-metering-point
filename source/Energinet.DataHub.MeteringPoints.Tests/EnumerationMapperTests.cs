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
using System.Globalization;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Helpers;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests
{
    [UnitTest]
    public class EnumerationMapperTests
    {
        private enum TestEnum
        {
            TeTest1,
            TeTest2,
            TeTest3,
        }

        private enum ZSettlementMethod
        {
            SmFlex = 0,
            SmProfiled = 1,
            SmNonprofiled = 2,
        }

        [Fact]
        public void Map_from_enum_to_EnumerationType_Success()
        {
            var testEnum = TestEnum.TeTest1;
            var enumerationType = testEnum.MapToEnumerationType<TestEnumerationType>();

            enumerationType.Id.Should().Be((int)TestEnum.TeTest1);
        }

        [Fact]
        public void Map_from_EnumerationType_to_enum_Success()
        {
            var enumerationType = new TestEnumerationType(1, nameof(TestEnumerationType.Test2));
            var testEnum = enumerationType.MapToEnum<TestEnum>();

            Convert.ToInt32(testEnum, CultureInfo.InvariantCulture).Should().Be(enumerationType.Id);
        }

        [Fact]
        public void MapZ_from_enum_to_EnumerationType_Success()
        {
            var testEnum = ZSettlementMethod.SmFlex;
            var enumerationType = testEnum.MapToEnumerationType<ZZSettlementMethod>();

            enumerationType.Id.Should().Be((int)TestEnum.TeTest1);
        }

        [Fact]
        public void MapZ_from_EnumerationType_to_enum_Success()
        {
            var enumerationType = new ZZSettlementMethod(1, nameof(ZZSettlementMethod.Flex));
            var testEnum = enumerationType.MapToEnum<ZSettlementMethod>();

            Convert.ToInt32(testEnum, CultureInfo.InvariantCulture).Should().Be(enumerationType.Id);
        }

        internal class TestEnumerationType : EnumerationType
        {
            public static readonly TestEnumerationType Test1 = new(0, nameof(Test1));
            public static readonly TestEnumerationType Test2 = new(1, nameof(Test2));
            public static readonly TestEnumerationType Test3 = new(2, nameof(Test3));

            public TestEnumerationType(int id, string name)
                : base(id, name)
            {
            }
        }

        internal class ZZSettlementMethod : EnumerationType
        {
            public static readonly ZZSettlementMethod Flex = new(0, nameof(Flex));
            public static readonly ZZSettlementMethod Profiled = new(1, nameof(Profiled));
            public static readonly ZZSettlementMethod NonProfiled = new(2, nameof(NonProfiled));

            public ZZSettlementMethod(int id, string name)
                : base(id, name)
            {
            }
        }
    }
}
