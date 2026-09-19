using FluentAssertions;
using Infra.Persistence;

namespace Tests.Unit.Persistence;

public class SqlServerConnectionStringTests
{
    [Fact]
    public void Create_WhenValid_BuildsLocalDockerStyleConnectionString()
    {
        var connectionString = SqlServerConnectionString.Create(
            "localhost",
            "TraderAnalyzerDb",
            "sa",
            "TraderAnalyzer_Dev!23");

        connectionString.Should().Be(
            "Server=localhost,1433;Database=TraderAnalyzerDb;User Id=sa;Password=TraderAnalyzer_Dev!23;TrustServerCertificate=True;Encrypt=True;");
    }

    [Fact]
    public void LocalDockerDevelopment_UsesLocalhostAndDefaultDatabase()
    {
        var connectionString = SqlServerConnectionString.LocalDockerDevelopment("TraderAnalyzer_Dev!23");

        connectionString.Should().Contain("Server=localhost,14333");
        connectionString.Should().Contain("Database=TraderAnalyzerDb");
        connectionString.Should().Contain("User Id=sa");
    }

    [Fact]
    public void Create_WhenHostMissing_Throws()
    {
        var act = () => SqlServerConnectionString.Create("", "TraderAnalyzerDb", "sa", "pw");

        act.Should().Throw<ArgumentException>();
    }
}
