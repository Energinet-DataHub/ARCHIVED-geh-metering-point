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
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Energinet.DataHub.MeteringPoints.ActorRegistrySync.Entities;
using Microsoft.Data.SqlClient;

namespace Energinet.DataHub.MeteringPoints.ActorRegistrySync.Services;

public class MeteringPointDbService : IDisposable
{
    private readonly SqlConnection _sqlConnection;
    private DbTransaction? _transaction;
    private bool _disposed;

    public MeteringPointDbService(string connectionString)
    {
        _sqlConnection = new SqlConnection(connectionString);
    }

    public async Task CleanUpAsync()
    {
        if (_transaction == null) await BeginTransactionAsync().ConfigureAwait(false);
        await _sqlConnection.ExecuteAsync("DELETE FROM [dbo].[GridAreaLinks]", transaction: _transaction)
            .ConfigureAwait(false);
        await _sqlConnection.ExecuteAsync("DELETE FROM [dbo].[GridAreas]", transaction: _transaction)
            .ConfigureAwait(false);
        await _sqlConnection.ExecuteAsync("DELETE FROM [dbo].[Actor]", transaction: _transaction)
            .ConfigureAwait(false);
    }

    public async Task InsertGriAreaLinkAsync(IEnumerable<GridAreaLink> gridAreaLinks)
    {
        if (gridAreaLinks == null) throw new ArgumentNullException(nameof(gridAreaLinks));

        if (_transaction == null) await BeginTransactionAsync().ConfigureAwait(false);

        var stringBuilder = new StringBuilder();
        foreach (var gridAreaLink in gridAreaLinks)
        {
            stringBuilder.Append("INSERT INTO [dbo].[GridAreaLinks] ([Id],[GridAreaId]) VALUES ('" + gridAreaLink.GridLinkId + "', '" + gridAreaLink.GridAreaId + "')");
            stringBuilder.AppendLine();
        }

        await _sqlConnection.ExecuteAsync(
            stringBuilder.ToString(),
            transaction: _transaction).ConfigureAwait(false);
    }

    public async Task InsertActorsAsync(IEnumerable<ActorRegistryActor> actors)
    {
        if (actors == null) throw new ArgumentNullException(nameof(actors));

        if (_transaction == null) await BeginTransactionAsync().ConfigureAwait(false);

        var stringBuilder = new StringBuilder();
        foreach (var actor in actors)
        {
            stringBuilder.Append(@"INSERT INTO [dbo].[Actor] ([Id], [IdentificationNumber],[IdentificationType], [Roles], [B2CId])
             VALUES ('" + actor.Id + "', '" + actor.IdentificationNumber + "', '" + actor.IdentificationType + "' , '" + MapFrom(actor.Roles) + "' , '" + actor.B2CId + "')");
            stringBuilder.AppendLine();
        }

        await _sqlConnection.ExecuteAsync(
            stringBuilder.ToString(),
            transaction: _transaction).ConfigureAwait(false);
    }

    public async Task InsertGridAreasAsync(IEnumerable<GridArea> gridAreas)
    {
        if (gridAreas == null) throw new ArgumentNullException(nameof(gridAreas));

        if (_transaction == null) await BeginTransactionAsync().ConfigureAwait(false);

        var stringBuilder = new StringBuilder();
        foreach (var gridArea in gridAreas)
        {
            stringBuilder.Append(
                @"INSERT INTO [dbo].[GridAreas]([Id],[Code],[Name],[ActorId])
                  VALUES ('" + gridArea.Id + "', '" + gridArea.Code + "', '" + gridArea.Name + "', '" + gridArea.ActorId + "')");
            stringBuilder.AppendLine();
        }

        await _sqlConnection.ExecuteAsync(
            stringBuilder.ToString(),
            transaction: _transaction).ConfigureAwait(false);
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync().ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _sqlConnection.Dispose();
        }

        _disposed = true;
    }

    private static string MapFrom(string? roles)
    {
        if (string.IsNullOrEmpty(roles))
        {
            return string.Empty;
        }

        var output = new List<string>();
        var rolesByNumber = roles.Split(',');

        foreach (var number in rolesByNumber)
        {
            switch (number)
            {
                case "1":
                    output.Add("BalanceResponsibleParty");
                    break;
                case "2":
                    output.Add("BalancingServiceProvider");
                    break;
                case "3":
                    output.Add("BillingAgent");
                    break;
                case "4":
                    output.Add("CapacityTrader");
                    break;
                case "5":
                    output.Add("Consumer");
                    break;
                case "6":
                    output.Add("ConsumptionResponsibleParty");
                    break;
                case "7":
                    output.Add("ConsentAdministrator");
                    break;
                case "8":
                    output.Add("CoordinatedCapacityCalculator");
                    break;
                case "9":
                    output.Add("CoordinationCentreOperator");
                    break;
                case "10":
                    output.Add("DataProvider");
                    break;
                case "11":
                    output.Add("EnergyServiceCompany");
                    break;
                case "12":
                    output.Add("EnergySupplier");
                    break;
                case "13":
                    output.Add("EnergyTrader");
                    break;
                case "14":
                    output.Add("GridAccessProvider");
                    break;
                case "15":
                    output.Add("ImbalanceSettlementResponsible");
                    break;
                case "16":
                    output.Add("InterconnectionTradeResponsible");
                    break;
                case "17":
                    output.Add("LfcOperator");
                    break;
                case "18":
                    output.Add("MarketInformationAggregator");
                    break;
                case "19":
                    output.Add("MarketOperator");
                    break;
                case "20":
                    output.Add("MeritOrderListResponsible");
                    break;
                case "21":
                    output.Add("MeterAdministrator");
                    break;
                case "22":
                    output.Add("MeterOperator");
                    break;
                case "23":
                    output.Add("MeteredDataAdministrator");
                    break;
                case "24":
                    output.Add("MeteredDataAggregator");
                    break;
                case "25":
                    output.Add("MeteredDataCollector");
                    break;
                case "26":
                    output.Add("MeteredDataResponsible");
                    break;
                case "27":
                    output.Add("MeteringPointAdministrator");
                    break;
                case "28":
                    output.Add("ModelMergingAgent");
                    break;
                case "29":
                    output.Add("ModellingAuthority");
                    break;
                case "30":
                    output.Add("NominatedElectricityMarketOperator");
                    break;
                case "31":
                    output.Add("NominationValidator");
                    break;
                case "32":
                    output.Add("PartyAdministrator");
                    break;
                case "33":
                    output.Add("PartyConnectedToTheGrid");
                    break;
                case "34":
                    output.Add("Producer");
                    break;
                case "35":
                    output.Add("ProductionResponsibleParty");
                    break;
                case "36":
                    output.Add("ReconciliationAccountable");
                    break;
                case "37":
                    output.Add("ReconciliationResponsible");
                    break;
                case "38":
                    output.Add("ReserveAllocator");
                    break;
                case "39":
                    output.Add("ResourceAggregator");
                    break;
                case "40":
                    output.Add("ResourceCapacityMechanismOperator");
                    break;
                case "41":
                    output.Add("ResourceProvider");
                    break;
                case "42":
                    output.Add("Scheduling");
                    break;
                case "43":
                    output.Add("Agent");
                    break;
                case "44":
                    output.Add("SchedulingAreaResponsible");
                    break;
                case "45":
                    output.Add("SystemOperator");
                    break;
                case "46":
                    output.Add("TradeResponsibleParty");
                    break;
                case "47":
                    output.Add("TransmissionCapacityAllocator");
                    break;
                case "48":
                    output.Add("DanishEnergyAgency");
                    break;
            }
        }

        return string.Join(',', output);
    }

    private async Task BeginTransactionAsync()
    {
        await _sqlConnection.OpenAsync().ConfigureAwait(false);
        _transaction = await _sqlConnection.BeginTransactionAsync().ConfigureAwait(false);
    }
}
