using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors
{
    #pragma warning disable SA1402 // These ErrorConverter types "overloaded" by type parameter are tightly coupled and it seems logical to have them in the same file.
    public abstract class ErrorConverter
    {
        public abstract Error Convert(ValidationError error);

        protected Error Default()
        {
            return new(string.Empty, string.Empty);
        }
    }

    public abstract class ErrorConverter<TError> : ErrorConverter
        where TError : ValidationError
    {
        public override Error Convert(ValidationError error)
        {
            return error is TError specificError
                ? Convert(specificError)
                : Default();
        }

        protected abstract Error Convert(TError error);
    }
    #pragma warning restore SA1402
}
