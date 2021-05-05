using System;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataBaseAccess.Write.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataBaseAccess
{
    /// <summary>
    /// EF Core Base Database Context
    /// </summary>
    public interface IBaseDatabseContext : IDisposable
    {
        /// <summary>
        /// Entity Framework DbSet for Outbox
        /// </summary>
        DbSet<OutboxMessagesDataModel> OutboxDataModels { get; set; }
    }
}