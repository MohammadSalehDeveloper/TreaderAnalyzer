using Core.Contracts.DTOs.Auth;
using Core.Domain.Entities;

namespace Core.Contracts.Interfaces.Security;

public interface ITokenService
{
    AuthTokensDto GenerateTokens(User user, string refreshTokenPlainText, DateTime refreshTokenExpiresAtUtc);

    string GenerateRefreshTokenPlainText();

    string HashToken(string plainTextToken);
}
