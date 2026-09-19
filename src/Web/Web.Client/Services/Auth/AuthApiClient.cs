using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Web.Client.Services.Auth;

public sealed class AuthApiException : Exception
{
    public AuthApiException(string message, HttpStatusCode statusCode, IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }

    public HttpStatusCode StatusCode { get; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; }
}

public interface IAuthApiClient
{
    Task<Core.Contracts.DTOs.Auth.LoginResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Core.Contracts.DTOs.Auth.LoginResponseDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
}

public sealed class AuthApiClient : IAuthApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;

    public AuthApiClient(HttpClient http)
    {
        _http = http;
    }

    public Task<Core.Contracts.DTOs.Auth.LoginResponseDto> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync<Core.Contracts.DTOs.Auth.LoginResponseDto>(HttpMethod.Post, "api/auth/login", request, cancellationToken);

    public Task<Core.Contracts.DTOs.Auth.LoginResponseDto> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync<Core.Contracts.DTOs.Auth.LoginResponseDto>(HttpMethod.Post, "api/auth/register", request, cancellationToken);

    public Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default) =>
        SendEmptyAsync(HttpMethod.Post, "api/auth/forgot-password", request, cancellationToken);

    public Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default) =>
        SendEmptyAsync(HttpMethod.Post, "api/auth/reset-password", request, cancellationToken);

    public Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default) =>
        SendEmptyAsync(HttpMethod.Post, "api/auth/logout", request, cancellationToken);

    private async Task<T> SendAsync<T>(
        HttpMethod method,
        string url,
        object body,
        CancellationToken cancellationToken)
    {
        using var response = await _http.SendAsync(CreateRequest(method, url, body), cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw await CreateExceptionAsync(response, cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        return payload ?? throw new AuthApiException("Empty response from auth API.", response.StatusCode);
    }

    private async Task SendEmptyAsync(
        HttpMethod method,
        string url,
        object body,
        CancellationToken cancellationToken)
    {
        using var response = await _http.SendAsync(CreateRequest(method, url, body), cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw await CreateExceptionAsync(response, cancellationToken);
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, object body) =>
        new(method, url) { Content = JsonContent.Create(body) };

    private static async Task<AuthApiException> CreateExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = response.ReasonPhrase ?? $"Request failed ({(int)response.StatusCode}).";
        IReadOnlyDictionary<string, string[]>? errors = null;

        if (!string.IsNullOrWhiteSpace(raw))
        {
            try
            {
                using var document = JsonDocument.Parse(raw);
                if (document.RootElement.TryGetProperty("detail", out var detail)
                    && detail.ValueKind == JsonValueKind.String
                    && !string.IsNullOrWhiteSpace(detail.GetString()))
                {
                    message = detail.GetString()!;
                }
                else if (document.RootElement.TryGetProperty("title", out var title)
                         && title.ValueKind == JsonValueKind.String
                         && !string.IsNullOrWhiteSpace(title.GetString()))
                {
                    message = title.GetString()!;
                }

                if (document.RootElement.TryGetProperty("errors", out var errorsElement)
                    && errorsElement.ValueKind == JsonValueKind.Object)
                {
                    var map = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
                    foreach (var property in errorsElement.EnumerateObject())
                    {
                        if (property.Value.ValueKind != JsonValueKind.Array)
                            continue;

                        map[property.Name] = property.Value.EnumerateArray()
                            .Where(item => item.ValueKind == JsonValueKind.String)
                            .Select(item => item.GetString()!)
                            .ToArray();
                    }

                    if (map.Count > 0)
                        errors = map;
                }
            }
            catch (JsonException)
            {
                // Keep status phrase.
            }
        }

        return new AuthApiException(message, response.StatusCode, errors);
    }
}
