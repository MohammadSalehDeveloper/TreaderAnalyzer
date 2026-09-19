using FluentAssertions;
using Infra.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Tests.Integration.Persistence;

public sealed class SqlServerContainerTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlServer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public Task InitializeAsync() => _sqlServer.StartAsync();

    public Task DisposeAsync() => _sqlServer.DisposeAsync().AsTask();

    [Fact]
    public async Task Migrate_OnSqlServer2022_CreatesAccountSchema()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_sqlServer.GetConnectionString())
            .Options;

        await using var db = new AppDbContext(options);

        await db.Database.MigrateAsync();

        (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
        (await db.Users.CountAsync()).Should().Be(0);
        (await db.Balances.CountAsync()).Should().Be(0);
        (await db.RefreshTokens.CountAsync()).Should().Be(0);
        (await db.PasswordResetTokens.CountAsync()).Should().Be(0);
    }
}
