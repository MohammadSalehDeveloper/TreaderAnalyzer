using Core.Contracts.DTOs.Auth;

namespace Core.Contracts.Interfaces.Security;

public interface IGoogleTokenValidator
{
    Task<GoogleUserInfoDto> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default);
}
