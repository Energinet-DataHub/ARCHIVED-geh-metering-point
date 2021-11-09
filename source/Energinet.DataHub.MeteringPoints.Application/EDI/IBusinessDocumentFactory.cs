using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
using Energinet.DataHub.MeteringPoints.Domain.EnergySuppliers;

namespace Energinet.DataHub.MeteringPoints.Application.EDI
{
    /// <summary>
    /// Create business documents.
    /// </summary>
    public interface IBusinessDocumentFactory
    {
        /// <summary>
        /// RSM 22
        /// </summary>
        void CreateAccountingPointCharacteristicsMessage(
            string requestTransactionId,
            MeteringPointDto meteringPointDto,
            GlnNumber energySupplierGlnNumber);
    }
}
