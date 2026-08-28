using Core.Domain.Common;
using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

public sealed class PasswordResetToken : Entity
{
    private PasswordResetToken()
    {
    }

    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public bool IsValid => UsedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;

    public static PasswordResetToken Create(Guid userId, string tokenHash, TimeSpan lifetime)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User id is required.");

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException("Token hash is required.");

        return new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAtUtc = DateTime.UtcNow.Add(lifetime),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkUsed()
    {
        if (UsedAtUtc is not null)
            throw new DomainException("Password reset token has already been used.");

        UsedAtUtc = DateTime.UtcNow;
    }
}
