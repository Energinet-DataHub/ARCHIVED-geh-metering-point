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
// using System.Linq;
// using Energinet.DataHub.MeteringPoints.Application;
// using Energinet.DataHub.MeteringPoints.Application.Authorization;
// using Energinet.DataHub.MeteringPoints.Application.Authorization.AuthorizationHandlers;
// using Energinet.DataHub.MeteringPoints.Infrastructure;
// using FluentAssertions;
// using SimpleInjector;
// using Xunit;
// using Xunit.Categories;
//
// namespace Energinet.DataHub.MeteringPoints.Tests.Authorization
// {
//     [UnitTest]
//     public class CreateMeteringPointInputValidationTests
//     {
//         [Fact]
//         public void Authorizations()
//         {
//             var container = new Container();
//
//             container.AddInputValidation();
//
//             var validators = container.GetAllInstances<IAuthorizationHandler<CreateMeteringPoint, CreateMeteringPointResult>>().Select(x => x.GetType());
//
//             var type = typeof(IAuthorizationHandler<CreateMeteringPoint, CreateMeteringPointResult>);
//             var types = type.Assembly
//                 .GetTypes()
//                 .Where(p => type.IsAssignableFrom(p));
//
//             types.Should().BeEquivalentTo(validators);
//         }
//
//         [Theory]
//         [InlineData("1550-10-21 25:12:12", false)]
//         public void OccurenceDate(string occurenceDate, bool expectToSucceed)
//         {
//             var createMeteringPoint = new CreateMeteringPoint
//             {
//                 OccurenceDate = occurenceDate,
//             };
//
//             var validator = new ExampleAuthorizationHandler();
//             var validationResult = validator.Validate(createMeteringPoint);
//
//             validationResult.Success.Should().Be(expectToSucceed);
//         }
//     }
// }
