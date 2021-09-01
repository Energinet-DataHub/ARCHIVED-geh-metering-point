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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork.Internals;

namespace Energinet.DataHub.MeteringPoints.Benchmarks.ReflectionStrategies
{
    public class CheckOnInvocationReflectionStrategy : ReflectionStrategy
    {
        internal override IEnumerable<T> GetAll<T>()
        {
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

            return fields.Select(f => f.GetValue(null)).Cast<T>();
        }

        internal override T FromName<T>(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            var matchingItem = Parse<T, string>(name, "name", item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return matchingItem;
        }

        internal override T FromValue<T>(int value)
        {
            var matchingItem = Parse<T, int>(value, "value", item => item.Id == value);
            return matchingItem;
        }

        internal override bool ContainsName<T>(string name)
        {
            var matchingItem = GetAll<T>()
                .FirstOrDefault(item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return matchingItem is not null;
        }

        internal override bool ContainsValue<T>(int value)
        {
            var matchingItem = GetAll<T>().FirstOrDefault(item => item.Id == value);
            return matchingItem is not null;
        }

        private T Parse<T, TValue>(TValue value, string description, Func<T, bool> predicate)
            where T : EnumerationType
        {
            var matchingItem = GetAll<T>().FirstOrDefault(predicate);

            return matchingItem ?? throw new InvalidOperationException($"'{value}' is not a valid {description} in {typeof(T)}");
        }
    }
}
