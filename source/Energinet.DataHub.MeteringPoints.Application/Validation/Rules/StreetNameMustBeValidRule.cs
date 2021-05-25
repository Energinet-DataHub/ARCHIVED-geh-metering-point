using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentValidation;
using FluentValidation.Results;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class StreetNameMustBeValidRule : AbstractValidator<CreateMeteringPoint>
    {
        private const int MaxStreetNameLength = 40;

        public StreetNameMustBeValidRule()
        {
            When(createMeteringPoint => createMeteringPoint.TypeOfMeteringPoint.Equals(MeteringPointType.Consumption.Name) || createMeteringPoint.TypeOfMeteringPoint.Equals(MeteringPointType.Production.Name), () =>
            {
                RuleFor_MandatoryForConsumptionAndProduction();
                RuleFor_StreetNameMaximumLength();
            })
            .Otherwise(RuleFor_StreetNameMaximumLength);
        }

        private void RuleFor_MandatoryForConsumptionAndProduction()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.InstallationLocationAddress.StreetName)
                .NotEmpty()
                .WithState(createMeteringPoint => new MandatoryFieldForMeteringPointTypeValidationError(nameof(createMeteringPoint.InstallationLocationAddress.StreetName), createMeteringPoint.TypeOfMeteringPoint));
        }

        private void RuleFor_StreetNameMaximumLength()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.InstallationLocationAddress.StreetName)
                .MaximumLength(MaxStreetNameLength)
                .WithState(createMeteringPoint => new MaximumLengthValidationError(nameof(createMeteringPoint.InstallationLocationAddress.StreetName), MaxStreetNameLength));
        }
    }
}
