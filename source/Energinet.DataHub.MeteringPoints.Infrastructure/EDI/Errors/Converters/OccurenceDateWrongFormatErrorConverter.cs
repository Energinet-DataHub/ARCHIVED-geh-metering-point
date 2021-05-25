using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors.Converters
{
    public class OccurenceDateWrongFormatErrorConverter : ErrorConverter<OccurenceDateWrongFormatValidationError>
    {
        // TODO: This is an example, redo when we know what/how etc.
        protected override Error Convert(OccurenceDateWrongFormatValidationError error)
        {
            return new("TODO", $"The occurrence data of a metering point should have format 'YYYY-MM-DD HH:MI:SSZ' (UTC+0)");
        }
    }
}
