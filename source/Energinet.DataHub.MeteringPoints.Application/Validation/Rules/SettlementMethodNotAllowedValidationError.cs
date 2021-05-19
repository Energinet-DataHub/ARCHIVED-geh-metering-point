using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class SettlementMethodNotAllowedValidationError : ValidationError
    {
        public SettlementMethodNotAllowedValidationError(string typeOfMeteringPoint)
        {
            TypeOfMeteringPoint = typeOfMeteringPoint;
        }

        public string TypeOfMeteringPoint { get; }
    }
}
