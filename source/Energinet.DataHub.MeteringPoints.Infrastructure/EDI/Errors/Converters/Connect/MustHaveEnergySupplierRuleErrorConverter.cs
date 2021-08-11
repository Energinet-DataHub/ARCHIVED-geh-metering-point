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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Rules.Connect;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors.Converters.Connect
{
    public class MustHaveEnergySupplierRuleErrorConverter : ErrorConverter<MustHaveEnergySupplierRuleError>
    {
        protected override ErrorMessage Convert(MustHaveEnergySupplierRuleError validationError)
        {
            if (validationError == null) throw new ArgumentNullException(nameof(validationError));
            //TODO: Must review this error message and corresponding rule. Is it still needed ? Should it be Energy Supplier and NOT Balance Supplier ?
            return new ErrorMessage(
                "D36",
                $"Metering Point {validationError.MeteringPointGsrn.Value} of type <2> cannot be connected: There is no Balance Supplier registered on the effectuation date <3>.");
        }
    }
}
