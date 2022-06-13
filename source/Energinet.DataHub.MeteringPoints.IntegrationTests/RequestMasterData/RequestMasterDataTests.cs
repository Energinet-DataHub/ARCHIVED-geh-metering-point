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
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.RequestMasterData;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.RequestMasterData;

public class RequestMasterDataTests : TestHost
{
    public RequestMasterDataTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
    }

    [Fact]
    public async Task Request_master_data_test()
    {
        var createCommand = Scenarios.CreateConsumptionMeteringPointCommand();
        await SendCommandAsync(createCommand).ConfigureAwait(false);

        var masterData = await QueryAsync(new GetMasterDataQuery(createCommand.GsrnNumber)).ConfigureAwait(false);

        Assert.Equal(createCommand.GsrnNumber, masterData.GsrnNumber);
        Assert.Equal(createCommand.StreetName, masterData.Address.StreetName);
        Assert.Equal(createCommand.StreetCode, masterData.Address.StreetCode);
        Assert.Equal(createCommand.CityName, masterData.Address.City);
        Assert.Equal(createCommand.FloorIdentification, masterData.Address.Floor);
        Assert.Equal(createCommand.RoomIdentification, masterData.Address.Room);
        Assert.Equal(createCommand.PostCode, masterData.Address.PostCode);
        Assert.Equal(createCommand.CountryCode, masterData.Address.CountryCode);
        Assert.Equal(createCommand.CitySubDivisionName, masterData.Address.CitySubDivision);
        Assert.Equal(createCommand.BuildingNumber, masterData.Address.BuildingNumber);
        Assert.Equal(int.Parse(createCommand.MunicipalityCode, CultureInfo.InvariantCulture), masterData.Address.MunicipalityCode);
        Assert.Equal(createCommand.IsActualAddress, masterData.Address.IsActualAddress);
        Assert.Equal(createCommand.GeoInfoReference, masterData.Address.GeoInfoReference.ToString());
        Assert.Equal(createCommand.LocationDescription, masterData.Address.LocationDescription);
        Assert.Equal(createCommand.ProductType, masterData.Series.Product);
        Assert.Equal(createCommand.UnitType, masterData.Series.UnitType);
        Assert.Equal(createCommand.MeteringGridArea, masterData.GridAreaDetails.Code);
        Assert.Empty(masterData.GridAreaDetails.FromCode);
        Assert.Empty(masterData.GridAreaDetails.ToCode);
        Assert.NotEmpty(masterData.ConnectionState);
        Assert.Equal(createCommand.MeteringMethod, masterData.MeteringMethod);
    }
}
