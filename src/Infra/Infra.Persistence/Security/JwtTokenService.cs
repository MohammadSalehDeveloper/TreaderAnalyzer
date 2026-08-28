using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Core.Contracts.DTOs.Auth;
using Core.Contracts.Interfaces.Security;
using Core.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infra.Persistence.Security;

public sealed class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthTokensDto GenerateTokens(
        User user,
        string refreshTokenPlainText,
        DateTime refreshTokenExpiresAtUtc)
    {
        var accessTokenLifetimeMinutes = int.TryParse(
            _configuration["Jwt:AccessTokenLifetimeMinutes"],
            out var configuredMinutes)
            ? configuredMinutes
            : 15;

        var accessTokenLifetime = TimeSpan.FromMinutes(accessTokenLifetimeMinutes);

        var expiresAtUtc = DateTime.UtcNow.Add(accessTokenLifetime);
        var accessToken = CreateAccessToken(user, expiresAtUtc);

        return new AuthTokensDto(
            accessToken,
            refreshTokenPlainText,
            expiresAtUtc,
            refreshTokenExpiresAtUtc);
    }

    public string GenerateRefreshTokenPlainText()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public string HashToken(string plainTextToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainTextToken));
        return Convert.ToHexString(bytes);
    }

    private string CreateAccessToken(User user, DateTime expiresAtUtc)
    {
        var signingKey = _configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");

        var issuer = _configuration["Jwt:Issuer"] ?? "TraderAnalyzer";
        var audience = _configuration["Jwt:Audience"] ?? "TraderAnalyzer";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
