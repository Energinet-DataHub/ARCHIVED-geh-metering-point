using System;

namespace Energinet.DataHub.MeteringPoints.Application.UserIdentity
{
    public class UserIdentityFactory
    {
        public UserIdentity FromString(string userIdentity)
        {
            if (userIdentity == null) throw new ArgumentNullException(nameof(userIdentity));
            return System.Text.Json.JsonSerializer.Deserialize<UserIdentity>(userIdentity);
        }
    }
}
