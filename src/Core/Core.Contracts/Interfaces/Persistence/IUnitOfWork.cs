namespace Core.Contracts.Interfaces.Persistence;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IBalanceRepository Balances { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
