using FluentAssertions;
using Infra.Persistence;

namespace Tests.Unit.Persistence;

public class SupportedDatabaseProvidersTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(SupportedDatabaseProviders.SqlServer)]
    [InlineData("sqlserver")]
    public void EnsureCurrentlySupported_WhenSqlServerOrOmitted_DoesNotThrow(string? provider)
    {
        var act = () => SupportedDatabaseProviders.EnsureCurrentlySupported(provider);

        act.Should().NotThrow();
        SupportedDatabaseProviders.IsSqlServer(provider).Should().BeTrue();
    }

    [Fact]
    public void EnsureCurrentlySupported_WhenPostgreSql_ThrowsNotSupported()
    {
        var act = () => SupportedDatabaseProviders.EnsureCurrentlySupported("PostgreSql");

        act.Should().Throw<NotSupportedException>()
            .WithMessage("*SqlServer*")
            .WithMessage("*PostgreSQL*");
    }
}
