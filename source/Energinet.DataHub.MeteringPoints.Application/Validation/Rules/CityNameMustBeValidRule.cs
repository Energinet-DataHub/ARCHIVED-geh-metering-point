using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class CityNameMustBeValidRule : AbstractValidator<CreateMeteringPoint>
    {
        private const int MaxCityNameLength = 25;

        public CityNameMustBeValidRule()
        {
            When(createMeteringPoint => createMeteringPoint.TypeOfMeteringPoint.Equals(MeteringPointType.Consumption.Name) || createMeteringPoint.TypeOfMeteringPoint.Equals(MeteringPointType.Production.Name), () =>
            {
                RuleFor_MandatoryForConsumptionAndProduction();
                RuleFor_CityNameMaximumLength();
            })
            .Otherwise(RuleFor_CityNameMaximumLength);
        }

        private void RuleFor_MandatoryForConsumptionAndProduction()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.InstallationLocationAddress.CityName)
                .NotEmpty()
                .WithState(createMeteringPoint => new MandatoryFieldForMeteringPointTypeValidationError(nameof(createMeteringPoint.InstallationLocationAddress.CityName), createMeteringPoint.TypeOfMeteringPoint));
        }

        private void RuleFor_CityNameMaximumLength()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.InstallationLocationAddress.CityName)
                .MaximumLength(MaxCityNameLength)
                .WithState(createMeteringPoint => new MaximumLengthValidationError(nameof(createMeteringPoint.InstallationLocationAddress.CityName), MaxCityNameLength));
        }
    }
}
