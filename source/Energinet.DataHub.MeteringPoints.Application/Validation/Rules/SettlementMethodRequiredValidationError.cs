using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class SettlementMethodRequiredValidationError : ValidationError
    {
        public SettlementMethodRequiredValidationError(string typeOfMeteringPoint)
        {
            TypeOfMeteringPoint = typeOfMeteringPoint;
        }

        public string TypeOfMeteringPoint { get; }
    }
}
