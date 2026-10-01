using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Infra.Persistence.Context;

namespace Infra.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configured = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        var password = Environment.GetEnvironmentVariable("Database__Password")
            ?? Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD");
        var connectionString = SqlServerConnectionString.ApplyPassword(
            string.IsNullOrWhiteSpace(configured)
                ? SqlServerConnectionString.LocalDockerWithoutPassword()
                : configured,
            password);

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
