using Core.Contracts.Interfaces.Security;
using Microsoft.AspNetCore.Identity;

namespace Infra.Persistence.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return _hasher.HashPassword(new object(), password);
    }

    public bool Verify(string password, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var result = _hasher.VerifyHashedPassword(new object(), passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
