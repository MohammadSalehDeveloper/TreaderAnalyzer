using Core.Contracts.Interfaces.Persistence;
using Core.Domain.Common;
using Infra.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Persistence.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    public virtual IQueryable<TEntity> Query() => DbSet.AsQueryable();
}
