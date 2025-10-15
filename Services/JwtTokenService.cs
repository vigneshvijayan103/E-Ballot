using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EBallotApi.Services
{
    public class JwtTokenService
    {
        public string GenerateToken(int userId, string email, string role)
        {
            // Load from environment variables

            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
            var expiryMinutes = Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES");

            if (string.IsNullOrEmpty(secretKey))
                throw new Exception("JWT_SECRET environment variable is not set.");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("UserId", userId.ToString()),
                new Claim(ClaimTypes.Email, email?.Trim() ?? string.Empty),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    string.IsNullOrEmpty(expiryMinutes) ? 60 : Convert.ToDouble(expiryMinutes)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
