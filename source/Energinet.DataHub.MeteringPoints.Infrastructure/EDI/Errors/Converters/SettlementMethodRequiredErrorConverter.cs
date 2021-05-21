using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors.Converters
{
    public class SettlementMethodRequiredErrorConverter : ErrorConverter<SettlementMethodRequiredValidationError>
    {
        // TODO: This is an example, redo when we know what/how etc.
        protected override Error Convert(SettlementMethodRequiredValidationError error)
        {
            return new("TODO", $"Settlementmethod required for meteringpoint type: {error.TypeOfMeteringPoint}");
        }
    }
}
