using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class MeteringPointTypeRequiredValidationError : ValidationError
    {
        public MeteringPointTypeRequiredValidationError(string meteringPointType)
        {
            MeteringPointType = meteringPointType;
        }

        public string MeteringPointType { get; }
    }
}
