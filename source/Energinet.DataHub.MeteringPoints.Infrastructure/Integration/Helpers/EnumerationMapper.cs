using System;
using System.Globalization;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Helpers
{
    public static class EnumerationMapper
    {
        public static TEnum MapToEnum<TEnum>(this EnumerationType enumType)
            where TEnum : struct, Enum
        {
            if (enumType is null)
            {
                throw new ArgumentNullException(nameof(enumType));
            }

            if (Enum.TryParse<TEnum>(enumType.Id.ToString(CultureInfo.InvariantCulture), out var parsedEnum))
            {
                return parsedEnum;
            }

            throw new InvalidOperationException("Could not map EnumerationType to enum");
        }

        public static TEnumerationType MapToEnumerationType<TEnumerationType>(this Enum enumss)
            where TEnumerationType : EnumerationType
        {
            if (enumss is null)
            {
                throw new ArgumentNullException(nameof(enumss));
            }

            var intVal = Convert.ToInt32(enumss, CultureInfo.InvariantCulture);
            return EnumerationType.FromValue<TEnumerationType>(intVal);
        }
    }
}
