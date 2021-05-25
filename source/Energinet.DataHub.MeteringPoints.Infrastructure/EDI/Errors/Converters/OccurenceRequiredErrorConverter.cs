using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors.Converters
{
    public class OccurenceRequiredErrorConverter : ErrorConverter<OccurenceRequiredValidationError>
    {
        // TODO: This is an example, redo when we know what/how etc.
        protected override Error Convert(OccurenceRequiredValidationError error)
        {
            return new("TODO", $"The occurrence of a metering point is mandatory");
        }
    }
}
