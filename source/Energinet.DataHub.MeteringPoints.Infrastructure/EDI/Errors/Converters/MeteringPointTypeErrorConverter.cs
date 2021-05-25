using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors.Converters
{
    public class MeteringPointTypeErrorConverter : ErrorConverter<MeteringPointTypeValidationError>
    {
        // TODO: This is an example, redo when we know what/how etc.
        protected override Error Convert(MeteringPointTypeValidationError error)
        {
            return new("TODO", $"The metering point type: {error.MeteringPointType}, is not valid");
        }
    }
}
