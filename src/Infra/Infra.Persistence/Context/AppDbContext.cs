using Core.Domain.Common;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infra.Persistence.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Balance> Balances => Set<Balance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, [modelBuilder]);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : AuditableEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(entity => !entity.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Modified && entry.Entity.UpdatedAtUtc is null)
            {
                entry.Property(nameof(AuditableEntity.UpdatedAtUtc)).CurrentValue = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
