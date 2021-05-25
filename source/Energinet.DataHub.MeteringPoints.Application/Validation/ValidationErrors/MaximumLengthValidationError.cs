using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors
{
    public class MaximumLengthValidationError : ValidationError
    {
        public MaximumLengthValidationError(string fieldName, int maxLength)
        {
            FieldName = fieldName;
            MaxLength = maxLength;
        }

        public string FieldName { get; }

        public int MaxLength { get; }
    }
}
