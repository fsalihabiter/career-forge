using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CareerForge.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace CareerForge.Api.Services;

public sealed class TokenService(
    IConfiguration configuration,
    UserManager<AppUser> users)
{
    public async Task<(string Token, DateTimeOffset ExpiresAt)> CreateAsync(
        AppUser user,
        string displayName)
    {
        var expires = DateTimeOffset.UtcNow.AddHours(1);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key eksik.")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new("name", displayName)
        };
        claims.AddRange((await users.GetRolesAsync(user))
            .Select(role => new Claim(ClaimTypes.Role, role)));
        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
