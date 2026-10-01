using Core.Contracts.DTOs.Users;

namespace Web.Client.Services.Account;

public interface IAccountApiClient
{
    Task<UserDto> GetProfileAsync(CancellationToken cancellationToken = default);
    Task<UserDto> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
}

public sealed class AccountApiClient : IAccountApiClient
{
    private readonly HttpClient _http;

    public AccountApiClient(HttpClient http)
    {
        _http = http;
    }

    public Task<UserDto> GetProfileAsync(CancellationToken cancellationToken = default) =>
        JsonApi.SendAsync<UserDto>(_http, HttpMethod.Get, "api/account/me", null, cancellationToken);

    public Task<UserDto> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default) =>
        JsonApi.SendAsync<UserDto>(_http, HttpMethod.Put, "api/account/profile", request, cancellationToken);

    public Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default) =>
        JsonApi.SendEmptyAsync(_http, HttpMethod.Put, "api/account/change-password", request, cancellationToken);
}
