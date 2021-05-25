using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors
{
    public class WrongFormatValidationError : ValidationError
    {
        public WrongFormatValidationError(string fieldName)
        {
            FieldName = fieldName;
        }

        public string FieldName { get; }
    }
}
