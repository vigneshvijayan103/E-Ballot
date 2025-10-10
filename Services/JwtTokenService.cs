using EBallotApi.Models;
using Microsoft.IdentityModel.Tokens;
using EBallotApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EBallotApi.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

                public string GenerateToken(int userId, string email, string role)
                     {
                        
                        var claims = new[]
                     {
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                        new Claim("UserId", userId.ToString()),
                       new Claim(ClaimTypes.Email, email?.Trim() ?? string.Empty),
                        new Claim(ClaimTypes.Role, role)
                    };




            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}


