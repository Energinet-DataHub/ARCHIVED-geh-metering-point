using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors.Converters
{
    public class SettlementMethodMissingRequiredDomainValuesErrorConverter : ErrorConverter<SettlementMethodMissingRequiredDomainValuesValidationError>
    {
        // TODO: This is an example, redo when we know what/how etc.
        protected override Error Convert(SettlementMethodMissingRequiredDomainValuesValidationError error)
        {
            return new("TODO", $"Missing required domain values in settlementmethod: {error.SettlementMethod}");
        }
    }
}
