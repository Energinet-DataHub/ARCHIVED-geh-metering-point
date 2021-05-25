using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors.Converters
{
    public class WrongFormatErrorConverter : ErrorConverter<WrongFormatValidationError>
    {
        // TODO: This is an example, redo when we know what/how etc.
        protected override Error Convert(WrongFormatValidationError error)
        {
            return new("TODO", $"Field with name: {error.FieldName} is in wrong format");
        }
    }
}
