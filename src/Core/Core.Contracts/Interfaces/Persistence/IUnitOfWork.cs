namespace Core.Contracts.Interfaces.Persistence;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IBalanceRepository Balances { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IPasswordResetTokenRepository PasswordResetTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
