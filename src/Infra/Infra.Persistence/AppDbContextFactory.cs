using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Infra.Persistence.Context;

namespace Infra.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=TraderAnalyzer;User Id=sa;Password=TraderAnalyzer_Dev!23;TrustServerCertificate=True;Encrypt=False");

        return new AppDbContext(optionsBuilder.Options);
    }
}
