// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
//
// using System;
// using System.Threading.Tasks;
// using Energinet.DataHub.MeteringPoints.Application.ChangeMasterData.Consumption;
// using Energinet.DataHub.MeteringPoints.Application.Common;
// using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
// using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
// using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
// using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
// using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
// using NodaTime;
// using Xunit;
// using Xunit.Categories;
//
// namespace Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData.ConsumptionMeteringPoints
// {
//     [IntegrationTest]
//     public class AddressChangeTests : TestHost
//     {
//         public AddressChangeTests(DatabaseFixture databaseFixture)
//             : base(databaseFixture)
//         {
//         }
//
//         [Fact]
//         public async Task Address_is_changed()
//         {
//             await CreateMeteringPointAsync().ConfigureAwait(false);
//             var request = CreateRequest()
//                 with
//                 {
//                     TransactionId = SampleData.Transaction,
//                     GsrnNumber = SampleData.GsrnNumber,
//                     Address = new Application.Address(
//                         StreetName: "New Street Name",
//                         PostCode: "6000",
//                         City: "New City Name",
//                         StreetCode: "0500",
//                         BuildingNumber: "4",
//                         CitySubDivision: "New",
//                         CountryCode: CountryCode.DK.Name,
//                         Floor: "9",
//                         Room: "9",
//                         MunicipalityCode: 999,
//                         IsActual: true,
//                         GeoInfoReference: Guid.NewGuid()),
//                 };
//
//             await InvokeBusinessProcessAsync(request).ConfigureAwait(false);
//
//             AssertConfirmMessage(DocumentType.ConfirmChangeMasterData);
//         }
//
//         [Fact]
//         public async Task Accept_when_no_changes_are_made()
//         {
//             await CreateMeteringPointAsync().ConfigureAwait(false);
//             var request = CreateRequest()
//                 with
//                 {
//                     TransactionId = SampleData.Transaction,
//                     GsrnNumber = SampleData.GsrnNumber,
//                     Address = Scenarios.CreateAddress(),
//                 };
//
//             await InvokeBusinessProcessAsync(request).ConfigureAwait(false);
//
//             AssertConfirmMessage(DocumentType.ConfirmChangeMasterData);
//         }
//
//         [Fact]
//         public async Task Reject_when_street_name_is_empty()
//         {
//             await CreateMeteringPointAsync().ConfigureAwait(false);
//
//             var request = CreateRequest()
//                 with
//                 {
//                     TransactionId = SampleData.Transaction,
//                     GsrnNumber = SampleData.GsrnNumber,
//                     Address = Scenarios.CreateAddress()
//                         with
//                         {
//                             StreetName = string.Empty,
//                         },
//                 };
//
//             await InvokeBusinessProcessAsync(request).ConfigureAwait(false);
//
//             AssertValidationError("E86");
//         }
//
//         [Fact]
//         public async Task Reject_if_post_code_is_empty()
//         {
//             await CreateMeteringPointAsync().ConfigureAwait(false);
//
//             var request = CreateRequest()
//                 with
//                 {
//                     TransactionId = SampleData.Transaction,
//                     GsrnNumber = SampleData.GsrnNumber,
//                     Address = Scenarios.CreateAddress()
//                         with
//                         {
//                             PostCode = string.Empty,
//                         },
//                 };
//
//             await InvokeBusinessProcessAsync(request).ConfigureAwait(false);
//
//             AssertValidationError("E86");
//         }
//
//         [Fact]
//         public async Task Reject_if_city_is_empty()
//         {
//             await CreateMeteringPointAsync().ConfigureAwait(false);
//
//             var request = CreateRequest()
//                 with
//                 {
//                     TransactionId = SampleData.Transaction,
//                     GsrnNumber = SampleData.GsrnNumber,
//                     Address = Scenarios.CreateAddress()
//                         with
//                         {
//                             City = string.Empty,
//                         },
//                 };
//
//             await InvokeBusinessProcessAsync(request).ConfigureAwait(false);
//
//             AssertValidationError("E86");
//         }
//
//         private MasterDataDocument CreateRequest()
//         {
//             var today = GetService<ISystemDateTimeProvider>().Now().ToDateTimeUtc();
//             var effectiveDate = Instant.FromUtc(today.Year, today.Month, today.Day, 22, 0, 0);
//             return TestUtils.CreateRequest()
//                 with
//                 {
//                     EffectiveDate = effectiveDate.ToString(),
//                 };
//         }
//
//         private Task<BusinessProcessResult> CreateMeteringPointAsync()
//         {
//             return InvokeBusinessProcessAsync(Scenarios.CreateConsumptionMeteringPointCommand());
//         }
//     }
// }
