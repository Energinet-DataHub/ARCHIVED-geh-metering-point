namespace Energinet.DataHub.MeteringPoints.Application.UserIdentity
{
    public class UserContext : IUserContext
    {
        public UserIdentity CurrentUser { get; set; }

        public string Key => "geh_userIdentity";
    }
}