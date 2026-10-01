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
            "unit-test-password");

        connectionString.Should().Be(
            "Server=localhost,1433;Database=TraderAnalyzerDb;User Id=sa;Password=unit-test-password;TrustServerCertificate=True;Encrypt=True;");
    }

    [Fact]
    public void LocalDockerDevelopment_UsesLocalhostAndDefaultDatabase()
    {
        var connectionString = SqlServerConnectionString.LocalDockerDevelopment("unit-test-password");

        connectionString.Should().Contain("Server=localhost,14333");
        connectionString.Should().Contain("Database=TraderAnalyzerDb");
        connectionString.Should().Contain("User Id=sa");
    }

    [Fact]
    public void ApplyPassword_WhenMissing_AppendsPassword()
    {
        var connectionString = SqlServerConnectionString.ApplyPassword(
            SqlServerConnectionString.LocalDockerWithoutPassword(),
            "unit-test-password");

        connectionString.Should().EndWith("Password=unit-test-password;");
        connectionString.Should().Contain("Server=localhost,14333");
    }

    [Fact]
    public void ApplyPassword_WhenAlreadyPresent_LeavesConnectionStringUnchanged()
    {
        const string configured = "Server=localhost,14333;Database=TraderAnalyzerDb;User Id=sa;Password=already-set;TrustServerCertificate=True;Encrypt=True;";

        var connectionString = SqlServerConnectionString.ApplyPassword(configured, "other-password");

        connectionString.Should().Be(configured);
    }

    [Fact]
    public void ApplyPassword_WhenPasswordMissing_Throws()
    {
        var act = () => SqlServerConnectionString.ApplyPassword(
            SqlServerConnectionString.LocalDockerWithoutPassword(),
            " ");

        act.Should().Throw<InvalidOperationException>().WithMessage("*appsettings*");
    }

    [Fact]
    public void Create_WhenHostMissing_Throws()
    {
        var act = () => SqlServerConnectionString.Create("", "TraderAnalyzerDb", "sa", "pw");

        act.Should().Throw<ArgumentException>();
    }
}
