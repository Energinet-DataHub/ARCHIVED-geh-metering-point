using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors.Converters
{
    public class SettlementMethodNotAllowedErrorConverter : ErrorConverter<SettlementMethodNotAllowedValidationError>
    {
        // TODO: This is an example, redo when we know what/how etc.
        protected override Error Convert(SettlementMethodNotAllowedValidationError error)
        {
            return new("TODO", $"Settlementmethod not allowed for meteringpoint type: {error.TypeOfMeteringPoint}");
        }
    }
}
