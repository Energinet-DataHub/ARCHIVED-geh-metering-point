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
using Energinet.DataHub.MeteringPoints.Benchmarks.ReflectionStrategies;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork.Internals;
using Energinet.DataHub.MeteringPoints.Tests.Assets;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.EnumerationTypeTest
{
    #pragma warning disable CA1034 // Nested classes
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

        [Theory]
        [InlineData(nameof(DocumentTypes.Lens), true)]
        [InlineData("name-not-known", false)]
        public void Given_a_document_type_When_contains_name_is_invoked_Then_it_match_the_result(string documentType, bool expected)
        {
            var sut = CreateStrategy();

            var result = sut.ContainsName<DocumentTypes>(documentType);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(5, true)]
        [InlineData(-1, false)]
        public void Given_a_document_type_When_contains_value_is_invoked_Then_it_match_the_result(
            int value,
            bool expected)
        {
            var sut = CreateStrategy();
            var result = sut.ContainsValue<DocumentTypes>(value);

            Assert.Equal(expected, result);
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

        public class ExpressionCacheReflectionStrategyTests : ReflectionStrategyTests
        {
            private const int DefaultCacheSize = 64;
            private const int CustomCacheSize = 1;

            [Fact]
            public void Given_cache_size_When_size_is_less_then_one_Then_argument_out_of_range_exception_is_thrown()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => new DictionaryCacheReflectionStrategy(0));
            }

            [Fact]
            public void Given_created_strategy_When_default_cache_size_Then_size_is_64()
            {
                var sut = new DictionaryCacheReflectionStrategy();

                Assert.Equal(DefaultCacheSize, sut.CacheSize);
            }

            [Fact]
            public void Given_created_strategy_When_custom_cache_size_Then_size_is_1()
            {
                var sut = new DictionaryCacheReflectionStrategy(CustomCacheSize);

                Assert.Equal(CustomCacheSize, sut.CacheSize);
            }

            [Fact(Skip = "Disable due to flackyness")]
            public void Given_multiple_enums_When_cache_size_is_exceed_Then_cache_is_resized()
            {
                var sut = new DictionaryCacheReflectionStrategy(CustomCacheSize);

                _ = sut.GetAll<DocumentTypes>();
                _ = sut.GetAll<OperationSystems>();

                // cache size incremented by the default value set in the constructor
                Assert.Equal(CustomCacheSize * 2, sut.CacheSize);
            }

            protected override ReflectionStrategy CreateStrategy() => new DictionaryCacheReflectionStrategy();
        }
    }
}
