using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ArkTracker.IntegrationTests.Helpers
{
    public static class JwtTokenHelper
    {
        public static string GenerateTestToken(string username = "testuser")
        {
            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes("this-is-a-very-secure-test-key-for-integration-tests"));
            SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new(
                issuer: "test-issuer",
                audience: "test-audience",
                claims: new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, username),
                    new Claim(ClaimTypes.Name, username)
                },
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
