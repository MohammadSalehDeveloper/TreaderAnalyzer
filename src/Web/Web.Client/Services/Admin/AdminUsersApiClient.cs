using Core.Contracts.DTOs.Users;
using Core.Domain.Enums;

namespace Web.Client.Services.Admin;

public interface IAdminUsersApiClient
{
    Task<IReadOnlyList<UserDto>> ListAsync(
        UserRole? role = null,
        UserStatus? status = null,
        CancellationToken cancellationToken = default);

    Task ActivateAsync(Guid userId, CancellationToken cancellationToken = default);

    Task SuspendAsync(Guid userId, CancellationToken cancellationToken = default);
}

public sealed class AdminUsersApiClient : IAdminUsersApiClient
{
    private readonly HttpClient _http;

    public AdminUsersApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<UserDto>> ListAsync(
        UserRole? role = null,
        UserStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var url = "api/admin/users";
        var query = new List<string>();

        if (role.HasValue)
            query.Add($"role={role.Value}");

        if (status.HasValue)
            query.Add($"status={status.Value}");

        if (query.Count > 0)
            url += "?" + string.Join("&", query);

        return await JsonApi.SendAsync<List<UserDto>>(_http, HttpMethod.Get, url, null, cancellationToken);
    }

    public Task ActivateAsync(Guid userId, CancellationToken cancellationToken = default) =>
        JsonApi.SendEmptyAsync(_http, HttpMethod.Post, $"api/admin/users/{userId}/activate", null, cancellationToken);

    public Task SuspendAsync(Guid userId, CancellationToken cancellationToken = default) =>
        JsonApi.SendEmptyAsync(_http, HttpMethod.Post, $"api/admin/users/{userId}/suspend", null, cancellationToken);
}
