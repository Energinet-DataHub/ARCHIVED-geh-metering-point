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
            CreateMapping<Application.CreateMeteringPoint>("MktActivityRecord", mapper => mapper
                .AddProperty(x => x.GsrnNumber, "MarketEvaluationPoint", "mRID")
                .AddProperty(x => x.AssetType, "MarketEvaluationPoint", "energyLabel_EnergyTechnologyAndFuel.technology")
                .AddProperty(x => x.MaximumPower, "MarketEvaluationPoint", "marketAgreement.contractedConnectionCapacity", "value")
                .AddProperty(x => x.MaximumCurrent, "MarketEvaluationPoint", "ratedCurrent")
                .AddProperty(x => x.TypeOfMeteringPoint, "MarketEvaluationPoint", "type")
                .AddProperty(x => x.SubTypeOfMeteringPoint, "MarketEvaluationPoint", "meteringMethod")
                .AddProperty(x => x.MeterReadingOccurrence, "MarketEvaluationPoint", "readCycle")
                .AddProperty(x => x.MeteringGridArea, "MarketEvaluationPoint", "meteringGridArea_Domain.mRID")
                .AddProperty(x => x.PowerPlant, "MarketEvaluationPoint", "Linked_MarketEvaluationPoint", "mRID")
                .AddProperty(x => x.LocationDescription, "MarketEvaluationPoint", "usagePointLocation.remark")
                .AddProperty(x => x.SettlementMethod, "MarketEvaluationPoint", "settlementMethod")
                .AddProperty(x => x.UnitType, "MarketEvaluationPoint", "Series", "quantity_Measure_Unit.name")
                .AddProperty(x => x.DisconnectionType, "MarketEvaluationPoint", "disconnectionMethod")
                .AddProperty(x => x.OccurenceDate, "start_DateAndOrTime.dateTime")
                .AddProperty(x => x.MeterNumber, "MarketEvaluationPoint", "meter.mRID")
                .AddProperty(x => x.TransactionId, "mRID")
                .AddProperty(x => x.PhysicalStatusOfMeteringPoint, "MarketEvaluationPoint", "connectionState")
                .AddProperty(x => x.NetSettlementGroup, "MarketEvaluationPoint", "netSettlementGroup")
                .AddProperty(x => x.ConnectionType, "MarketEvaluationPoint", "mPConnectionType")
                .AddProperty(x => x.InstallationLocationAddress, "sdf"));
        }
    }
}
