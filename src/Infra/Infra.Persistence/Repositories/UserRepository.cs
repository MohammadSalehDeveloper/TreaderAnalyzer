using Core.Contracts.Interfaces.Persistence;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Infra.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByIdWithBalanceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(user => user.Balance)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();

        return await DbSet
            .Include(user => user.Balance)
            .FirstOrDefaultAsync(user => user.Email == normalized, cancellationToken);
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var normalized = userName.Trim();

        return await DbSet
            .Include(user => user.Balance)
            .FirstOrDefaultAsync(user => user.UserName == normalized, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();

        return await DbSet.AnyAsync(
            user => user.Email == normalized && (!excludeUserId.HasValue || user.Id != excludeUserId.Value),
            cancellationToken);
    }

    public async Task<bool> UserNameExistsAsync(
        string userName,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = userName.Trim();

        return await DbSet.AnyAsync(
            user => user.UserName == normalized && (!excludeUserId.HasValue || user.Id != excludeUserId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyList<User>> ListAsync(
        UserRole? role = null,
        UserStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().Include(user => user.Balance).AsQueryable();

        if (role.HasValue)
            query = query.Where(user => user.Role == role.Value);

        if (status.HasValue)
            query = query.Where(user => user.Status == status.Value);

        return await query
            .OrderBy(user => user.UserName)
            .ToListAsync(cancellationToken);
    }
}
