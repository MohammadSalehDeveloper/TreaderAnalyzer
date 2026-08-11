using Core.Domain.Entities;

namespace Core.Contracts.Interfaces.Persistence;

public interface IBalanceRepository : IRepository<Balance>
{
    Task<Balance?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
