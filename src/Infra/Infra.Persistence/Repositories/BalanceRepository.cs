using Core.Contracts.Interfaces.Persistence;
using Core.Domain.Entities;
using Infra.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Persistence.Repositories;

public class BalanceRepository : Repository<Balance>, IBalanceRepository
{
    public BalanceRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Balance?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(balance => balance.UserId == userId, cancellationToken);
    }
}
