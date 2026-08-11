using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Contracts.Interfaces.Persistence;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByIdWithBalanceAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);

    Task<bool> UserNameExistsAsync(string userName, Guid? excludeUserId = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> ListAsync(
        UserRole? role = null,
        UserStatus? status = null,
        CancellationToken cancellationToken = default);
}
