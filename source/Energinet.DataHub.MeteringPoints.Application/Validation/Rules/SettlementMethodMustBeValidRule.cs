using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentValidation;
using FluentValidation.Validators;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class SettlementMethodMustBeValidRule : AbstractValidator<CreateMeteringPoint>
    {
        /*The settlement method of a metering point is mandatory if the MP is E17 (consumption) or D13, otherwise it is not allowed
 - The settlement method of a metering point has domain values E02 (Non profiled), D01 (Flex)*/

        public SettlementMethodMustBeValidRule()
        {
            When(point => point.TypeOfMeteringPoint.Equals("Consumption") || point.TypeOfMeteringPoint.Equals("NetLossCorrection"), () =>
            {
                RuleFor(createMeteringPoint => createMeteringPoint.SettlementMethod)
                    .NotEmpty()
                    .WithState(createMeteringPoint => new SettlementMethodNotAllowedValidationError(createMeteringPoint.TypeOfMeteringPoint));
                RuleFor(createMeteringPoint => createMeteringPoint.SettlementMethod)
                    .Must(settlementMethod => settlementMethod.Contains("NonProfiled") || settlementMethod.Contains("Flex"))
                    .WithState(createMeteringPoint => new SettlementMethodMissingRequiredDomainValuesValidationError(createMeteringPoint.SettlementMethod));
            }).Otherwise(() =>
            {
                RuleFor(createMeteringPoint => createMeteringPoint.SettlementMethod)
                    .Empty()
                    .WithState(createMeteringPoint => new SettlementMethodNotAllowedValidationError(createMeteringPoint.TypeOfMeteringPoint));
            });
        }
    }
}
