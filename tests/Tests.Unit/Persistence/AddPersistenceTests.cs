using FluentAssertions;
using Infra.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Unit.Persistence;

public class AddPersistenceTests
{
    [Fact]
    public void AddPersistence_WhenConnectionStringMissing_Throws()
    {
        var services = new ServiceCollection();

        var act = () => services.AddPersistence(" ", SupportedDatabaseProviders.SqlServer);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddPersistence_WhenProviderIsPostgreSql_Throws()
    {
        var services = new ServiceCollection();

        var act = () => services.AddPersistence(
            SqlServerConnectionString.LocalDockerDevelopment("TraderAnalyzer_Dev!23"),
            "PostgreSql");

        act.Should().Throw<NotSupportedException>();
    }
}
