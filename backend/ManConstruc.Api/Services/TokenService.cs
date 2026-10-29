using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ManConstruc.Api.Contracts;
using ManConstruc.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace ManConstruc.Api.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthResponse CreateAuthResponse(User user)
    {
        var minutes = _configuration.GetValue("Jwt:ExpiresMinutes", 480);
        var expiresAt = DateTime.UtcNow.AddMinutes(minutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("name", user.FullName),
            new("company", user.CompanyName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetKey()));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "ManConstruc",
            audience: _configuration["Jwt:Audience"] ?? "ManConstruc.Spa",
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        var serialized = new JwtSecurityTokenHandler().WriteToken(token);
        var userDto = new UserDto(user.Id, user.Email, user.FullName, user.CompanyName);

        return new AuthResponse(serialized, expiresAt, userDto);
    }

    private string GetKey()
    {
        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException("Jwt:Key debe tener al menos 32 bytes.");
        }

        return key;
    }
}
