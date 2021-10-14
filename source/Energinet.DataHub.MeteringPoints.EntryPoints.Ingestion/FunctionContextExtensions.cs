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
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion
{
    /// <summary>
    /// https://github.com/Azure/azure-functions-dotnet-worker/issues/414
    /// </summary>
    public static class FunctionContextExtensions
    {
#pragma warning disable CA1031 // TODO: we catch all as this try/catch shouldn't be here long, see note
        internal static HttpRequestData? GetHttpRequestData(this FunctionContext functionContext)
        {
            if (functionContext == null) throw new ArgumentNullException(nameof(functionContext));

            try
            {
                KeyValuePair<Type, object> keyValuePair = functionContext.Features.SingleOrDefault(f => f.Key.Name == "IFunctionBindingsFeature");
                object functionBindingsFeature = keyValuePair.Value;
                Type type = functionBindingsFeature.GetType();
                var inputData = type.GetProperties().Single(p => p.Name == "InputData").GetValue(functionBindingsFeature) as IReadOnlyDictionary<string, object>;
                return inputData?.Values.SingleOrDefault(o => o is HttpRequestData) as HttpRequestData;
            }
            catch
            {
                return null;
            }
        }
    }
}
