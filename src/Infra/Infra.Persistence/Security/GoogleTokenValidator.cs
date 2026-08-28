using Core.Contracts.DTOs.Auth;
using Core.Contracts.Interfaces.Security;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace Infra.Persistence.Security;

public sealed class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly string _clientId;

    public GoogleTokenValidator(IConfiguration configuration)
    {
        _clientId = configuration["Google:ClientId"]
            ?? throw new InvalidOperationException("Google:ClientId is not configured.");
    }

    public async Task<GoogleUserInfoDto> ValidateIdTokenAsync(
        string idToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_clientId]
                });

            var firstName = payload.GivenName ?? string.Empty;
            var lastName = payload.FamilyName ?? string.Empty;

            if (string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(payload.Name))
            {
                var parts = payload.Name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                firstName = parts.Length > 0 ? parts[0] : "Google";
                lastName = parts.Length > 1 ? parts[1] : "User";
            }

            if (string.IsNullOrWhiteSpace(firstName))
                firstName = "Google";

            if (string.IsNullOrWhiteSpace(lastName))
                lastName = "User";

            return new GoogleUserInfoDto(
                payload.Subject,
                payload.Email,
                firstName,
                lastName,
                payload.EmailVerified);
        }
        catch (InvalidJwtException exception)
        {
            throw new InvalidOperationException("Invalid Google ID token.", exception);
        }
    }
}
