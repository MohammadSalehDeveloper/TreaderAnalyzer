using Core.Contracts.Interfaces.Persistence;
using Core.Domain.Entities;
using Infra.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Persistence.Repositories;

public class PasswordResetTokenRepository : Repository<PasswordResetToken>, IPasswordResetTokenRepository
{
    public PasswordResetTokenRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<PasswordResetToken?> GetValidByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(
            token => token.TokenHash == tokenHash
                && token.UsedAtUtc == null
                && token.ExpiresAtUtc > DateTime.UtcNow,
            cancellationToken);
    }

    public async Task InvalidateAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await DbSet
            .Where(token => token.UserId == userId && token.UsedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
            token.MarkUsed();
    }
}
