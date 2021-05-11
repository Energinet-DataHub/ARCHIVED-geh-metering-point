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

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork.Internals;

namespace Energinet.DataHub.MeteringPoints.Benchmarks
{
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public class ReflectionStrategyBenchmarks
    {
        private readonly CacheAllReflectionStrategy _cacheAll;
        private readonly CheckOnInvocationReflectionStrategy _checkOnInvocation;
        private readonly DictionaryCacheReflectionStrategy _dictionaryCache;

        public ReflectionStrategyBenchmarks()
        {
            _checkOnInvocation = new CheckOnInvocationReflectionStrategy();
            _cacheAll = new CacheAllReflectionStrategy();
            _dictionaryCache = new DictionaryCacheReflectionStrategy();
        }

        [Benchmark(Baseline = true, Description = nameof(CheckOnInvocationReflectionStrategy))]
        [BenchmarkCategory("GetAll")]
        public void CheckOnInvocation_GetAll() => _checkOnInvocation.GetAll<DocumentTypes>();

        [Benchmark(Description = nameof(CacheAllReflectionStrategy))]
        [BenchmarkCategory("GetAll")]
        public void CacheAll_GetAll() => _cacheAll.GetAll<DocumentTypes>();

        [Benchmark(Description = nameof(DictionaryCacheReflectionStrategy))]
        [BenchmarkCategory("GetAll")]
        public void ExpressionCache_GetAll() => _dictionaryCache.GetAll<DocumentTypes>();

        [Benchmark(Baseline = true, Description = nameof(CheckOnInvocationReflectionStrategy))]
        [BenchmarkCategory("FromName")]
        public void CheckOnInvocation_FromName() => _checkOnInvocation.FromName<DocumentTypes>("Lens");

        [Benchmark(Description = nameof(CacheAllReflectionStrategy))]
        [BenchmarkCategory("FromName")]
        public void CacheAll_FromName() => _cacheAll.FromName<DocumentTypes>("Lens");

        [Benchmark(Description = nameof(DictionaryCacheReflectionStrategy))]
        [BenchmarkCategory("FromName")]
        public void ExpressionCache_FromName() => _dictionaryCache.FromName<DocumentTypes>("Lens");

        [Benchmark(Baseline = true, Description = nameof(CheckOnInvocationReflectionStrategy))]
        [BenchmarkCategory("FromValue")]
        public void CheckOnInvocation_FromValue() => _checkOnInvocation.FromValue<DocumentTypes>(5);

        [Benchmark(Description = nameof(CacheAllReflectionStrategy))]
        [BenchmarkCategory("FromValue")]
        public void CacheAll_FromValue() => _cacheAll.FromValue<DocumentTypes>(5);

        [Benchmark(Description = nameof(DictionaryCacheReflectionStrategy))]
        [BenchmarkCategory("FromValue")]
        public void ExpressionCache_FromValue() => _dictionaryCache.FromValue<DocumentTypes>(5);
    }
}
