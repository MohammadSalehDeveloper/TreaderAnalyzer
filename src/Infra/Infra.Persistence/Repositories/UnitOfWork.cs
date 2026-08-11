using Core.Contracts.Interfaces.Persistence;
using Infra.Persistence.Context;

namespace Infra.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(
        AppDbContext context,
        IUserRepository users,
        IBalanceRepository balances)
    {
        _context = context;
        Users = users;
        Balances = balances;
    }

    public IUserRepository Users { get; }
    public IBalanceRepository Balances { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
