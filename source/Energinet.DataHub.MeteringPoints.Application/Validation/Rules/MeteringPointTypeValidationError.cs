using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class MeteringPointTypeValidationError : ValidationError
    {
        public MeteringPointTypeValidationError(string meteringPointType)
        {
            MeteringPointType = meteringPointType;
        }

        public string MeteringPointType { get; }
    }
}
