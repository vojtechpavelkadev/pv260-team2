using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ArkTracker.IntegrationTests.Helpers
{
    public static class JwtTokenHelper
    {
        public static string GenerateTestToken(string username = "testuser")
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this-is-a-very-secure-test-key-for-integration-tests"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
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
