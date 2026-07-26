using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CareerForge.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace CareerForge.Api.Services;

public sealed class TokenService(IConfiguration configuration)
{
    public (string Token, DateTimeOffset ExpiresAt) Create(AppUser user, string displayName)
    {
        var expires = DateTimeOffset.UtcNow.AddHours(1);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key eksik.")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim("name", displayName)
            ],
            expires: expires.UtcDateTime,
            signingCredentials: credentials);
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
