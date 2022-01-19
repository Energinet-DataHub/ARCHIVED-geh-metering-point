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

using Energinet.DataHub.MeteringPoints.Application.Validation.Extensions;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.ChangeMasterData
{
    public class AddressValidator : AbstractValidator<Address?>
    {
        public AddressValidator()
        {
            When(address => address is not null, () =>
            {
                RuleFor(address => address!)
                    .CheckRules(address => Domain.MasterDataHandling.Components.Addresses.Address.CheckRules(
                        address?.StreetName,
                        address?.StreetCode,
                        address?.BuildingNumber,
                        address?.City,
                        address?.CitySubDivision,
                        address?.PostCode,
                        string.IsNullOrWhiteSpace(address?.CountryCode) ? null : EnumerationType.FromName<CountryCode>(address.CountryCode),
                        address?.Floor,
                        address?.Room,
                        address?.MunicipalityCode,
                        address?.LocationDescription));
            });
        }
    }
}
