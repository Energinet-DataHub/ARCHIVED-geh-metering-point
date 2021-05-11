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
using Energinet.DataHub.MeteringPoints.Domain.SeedWork.Internals;
using Energinet.DataHub.MeteringPoints.Tests.Assets;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests
{
    [UnitTest]
    public abstract class ReflectionStrategyTests
    {
        [Fact]
        public void Given_document_types_When_lookup_name_Then_find_a_match()
        {
            var sut = CreateStrategy();
            var expected = DocumentTypes.Lens;
            var actual = sut.FromName<DocumentTypes>("Lens");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Given_document_types_When_lookup_name_Then_throws_InvalidOperationException()
        {
            var sut = CreateStrategy();

            Assert.Throws<InvalidOperationException>(() => sut.FromName<DocumentTypes>("Photo"));
        }

        [Fact]
        public void Given_document_types_When_lookup_value_Then_find_a_match()
        {
            var sut = CreateStrategy();
            var expected = DocumentTypes.Lens;
            var actual = sut.FromValue<DocumentTypes>(5);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Given_document_types_When_lookup_value_Then_throws_InvalidOperationException()
        {
            var sut = CreateStrategy();

            Assert.Throws<InvalidOperationException>(() => sut.FromValue<DocumentTypes>(int.MaxValue));
        }

        [Fact]
        public void Given_document_types_When_get_all_Then_find_all_fields()
        {
            var sut = CreateStrategy();

            var fields = sut.GetAll<DocumentTypes>().ToArray();

            Assert.NotEmpty(fields);
            Assert.Equal(5, fields.Length);
            Assert.Contains(DocumentTypes.Lens, fields);
        }

        protected abstract ReflectionStrategy CreateStrategy();

        public class CheckOnInvocationReflectionStrategyTests : ReflectionStrategyTests
        {
            protected override ReflectionStrategy CreateStrategy() => new CheckOnInvocationReflectionStrategy();
        }

        public class CacheAllReflectionStrategyTests : ReflectionStrategyTests
        {
            protected override ReflectionStrategy CreateStrategy() => new CacheAllReflectionStrategy();
        }

        public class CachePerMethodReflectionStrategyTests : ReflectionStrategyTests
        {
            protected override ReflectionStrategy CreateStrategy() => new CachePerMethodReflectionStrategy();
        }
    }
}
