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

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter.Mappings
{
    public class CreateMeteringPointXmlMappingConfiguration : XmlMappingConfigurationBase
    {
        public CreateMeteringPointXmlMappingConfiguration()
        {
            CreateMapping<Application.CreateMeteringPoint>(mapper => mapper
                .AddProperty(x => x.GsrnNumber, "mRID")
                .AddProperty(x => x.AssetType, "energyLabel_EnergyTechnologyAndFuel.technology")
                .AddProperty(x => x.MaximumPower, "marketAgreement.contractedConnectionCapacity")
                .AddProperty(x => x.MaximumCurrent, "ratedCurrent")
                .AddProperty(x => x.TypeOfMeteringPoint, "type")
                .AddProperty(x => x.SubTypeOfMeteringPoint, "meteringMethod")
                .AddProperty(x => x.MeterReadingOccurrence, "readCycle")
                .AddProperty(x => x.MeteringGridArea, "meteringGridArea_Domain.mRID")
                .AddProperty(x => x.PowerPlant, "Linked_MarketEvaluationPoint") // ... mRID
                .AddProperty(x => x.LocationDescription, "usagePointLocation.remark")
                .AddProperty(x => x.SettlementMethod, "settlementMethod")
                .AddProperty(x => x.UnitType, "quantity_Measure_Unit.name")
                .AddProperty(x => x.DisconnectionType, "disconnectionMethod")
                .AddProperty(x => x.OccurenceDate, "start_DateAndOrTime.dateTime")
                .AddProperty(x => x.MeterNumber, "meter.mRID")
                .AddProperty(x => x.TransactionId, "mRID")
                .AddProperty(x => x.PhysicalStatusOfMeteringPoint, "connectionState")
                .AddProperty(x => x.NetSettlementGroup, "netSettlementGroup")
                .AddProperty(x => x.ConnectionType, "mPConnectionType")
                .AddProperty(x => x.InstallationLocationAddress, "sdf"));
        }
    }
}
