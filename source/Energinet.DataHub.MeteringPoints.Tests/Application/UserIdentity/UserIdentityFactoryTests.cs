using System;
using Energinet.DataHub.MeteringPoints.Application.UserIdentity;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests.Application.UserIdentity
{
    public class UserIdentityFactoryTests
    {
        [Theory]
        [InlineData("{\"geh_userIdentity\":\"{\\\"Id\\\":\\\"5\\\"}\"}", "geh_userIdentity", "5")]
        [InlineData("{\"other_userIdentity\":\"{\\\"Id\\\":\\\"5\\\"}\", \"geh_userIdentity\":\"{\\\"Id\\\":\\\"5\\\"}\"}", "geh_userIdentity", "5")]
        public void ConvertToUserIdentityFromDictionaryString(string inputText, string propertyKey, string expectedUserId)
        {
            // Arrange
            var userIdentityFactory = new UserIdentityFactory();

            // Act
            var userIdentityParsed = userIdentityFactory.FromDictionaryString(inputText, propertyKey);

            // Assert
            Assert.Equal(expectedUserId, userIdentityParsed.Id);
        }

        [Theory]
        [InlineData("{\"123geh_userIdentity\":\"{\\\"Id\\\":\\\"5\\\"}\"}", "geh_userIdentity")]
        public void UserIdentityArgumentNullException(string inputText, string propertyKey)
        {
            // Arrange
            var userIdentityFactory = new UserIdentityFactory();

            // Act - Assert
            Assert.Throws<ArgumentNullException>(() =>
                userIdentityFactory.FromDictionaryString(inputText, propertyKey));
        }
    }
}
