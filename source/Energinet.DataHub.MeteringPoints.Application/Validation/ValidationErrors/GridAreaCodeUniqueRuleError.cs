using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors
{
    public class GridAreaCodeUniqueRuleError : ValidationError
    {
        public GridAreaCodeUniqueRuleError(string code)
        {
            Code = code;
        }

        public string Code { get; }
    }
}
