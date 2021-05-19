using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class SettlementMethodMissingRequiredDomainValuesValidationError : ValidationError
    {
        public SettlementMethodMissingRequiredDomainValuesValidationError(string settlementMethod)
        {
            SettlementMethod = settlementMethod;
        }

        public string SettlementMethod { get; }
    }
}
