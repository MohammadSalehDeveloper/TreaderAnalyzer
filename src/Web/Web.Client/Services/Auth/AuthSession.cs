using System.Net.Http.Headers;
using System.Text.Json;
using Core.Contracts.DTOs.Auth;
using Core.Contracts.DTOs.Users;
using Microsoft.JSInterop;

namespace Web.Client.Services.Auth;

public interface IAuthSession
{
    bool IsAuthenticated { get; }
    UserDto? User { get; }
    AuthTokensDto? Tokens { get; }
    event Action? Changed;
    Task InitializeAsync();
    Task SetSessionAsync(LoginResponseDto response);
    Task ClearAsync();
}

public sealed class AuthSession : IAuthSession
{
    private const string StorageKey = "traderanalyzer.auth";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IJSRuntime _js;
    private readonly HttpClient _http;

    public AuthSession(IJSRuntime js, HttpClient http)
    {
        _js = js;
        _http = http;
    }

    public bool IsAuthenticated => Tokens is not null && User is not null;
    public UserDto? User { get; private set; }
    public AuthTokensDto? Tokens { get; private set; }
    public event Action? Changed;

    public async Task InitializeAsync()
    {
        try
        {
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (string.IsNullOrWhiteSpace(json))
                return;

            var stored = JsonSerializer.Deserialize<StoredAuth>(json, JsonOptions);
            if (stored?.Tokens is null || stored.User is null)
                return;

            Tokens = stored.Tokens;
            User = stored.User;
            ApplyBearer();
            Changed?.Invoke();
        }
        catch
        {
            await ClearAsync();
        }
    }

    public async Task SetSessionAsync(LoginResponseDto response)
    {
        Tokens = response.Tokens;
        User = response.User;
        ApplyBearer();

        var json = JsonSerializer.Serialize(new StoredAuth(response.Tokens, response.User), JsonOptions);
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        Changed?.Invoke();
    }

    public async Task ClearAsync()
    {
        Tokens = null;
        User = null;
        _http.DefaultRequestHeaders.Authorization = null;

        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        }
        catch
        {
            // Ignore storage failures during logout.
        }

        Changed?.Invoke();
    }

    private void ApplyBearer()
    {
        if (Tokens is null)
        {
            _http.DefaultRequestHeaders.Authorization = null;
            return;
        }

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Tokens.AccessToken);
    }

    private sealed record StoredAuth(AuthTokensDto Tokens, UserDto User);
}
