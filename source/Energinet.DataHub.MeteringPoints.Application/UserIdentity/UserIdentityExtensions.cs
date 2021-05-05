namespace Energinet.DataHub.MeteringPoints.Application.UserIdentity
{
    public static class UserIdentityExtensions
    {
        public static string AsString(this UserIdentity userIdentity)
        {
            return System.Text.Json.JsonSerializer.Serialize(userIdentity);
        }
    }
}
