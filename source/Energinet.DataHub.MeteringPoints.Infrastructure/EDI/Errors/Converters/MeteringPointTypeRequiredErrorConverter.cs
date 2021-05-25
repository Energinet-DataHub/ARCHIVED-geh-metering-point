using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors.Converters
{
    public class MeteringPointTypeRequiredErrorConverter : ErrorConverter<MeteringPointTypeRequiredValidationError>
    {
        // TODO: This is an example, redo when we know what/how etc.
        protected override Error Convert(MeteringPointTypeRequiredValidationError error)
        {
            return new("TODO", $"The type of a metering point is mandatory");
        }
    }
}
