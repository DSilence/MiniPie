using MiniPie.Core.SpotifyWeb.Models;
using Xunit;

namespace MiniPie.Tests.Core.SpotifyWeb
{
    public class GrantTypeTest
    {
        [Theory]
        [InlineData(GrantType.AuthorizationCode, "authorization_code")]
        [InlineData(GrantType.RefreshToken, "refresh_token")]
        public void TestGetDescription(GrantType type, string expected)
        {
            Assert.Equal(expected, type.GetDescription());
        }
    }
}
