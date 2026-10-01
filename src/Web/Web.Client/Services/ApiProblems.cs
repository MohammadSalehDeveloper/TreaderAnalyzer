using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Web.Client.Services;

public sealed class ApiException : Exception
{
    public ApiException(string message, HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}

internal static class JsonApi
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<T> SendAsync<T>(
        HttpClient http,
        HttpMethod method,
        string url,
        object? body,
        CancellationToken cancellationToken)
    {
        using var response = await http.SendAsync(CreateRequest(method, url, body), cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw await ReadAsync(response, cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<T>(Options, cancellationToken);
        return payload ?? throw new ApiException("Empty response from the API.", response.StatusCode);
    }

    public static async Task SendEmptyAsync(
        HttpClient http,
        HttpMethod method,
        string url,
        object? body,
        CancellationToken cancellationToken)
    {
        using var response = await http.SendAsync(CreateRequest(method, url, body), cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw await ReadAsync(response, cancellationToken);
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, object? body)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null)
            request.Content = JsonContent.Create(body);

        return request;
    }

    private static async Task<ApiException> ReadAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = response.ReasonPhrase ?? $"Request failed ({(int)response.StatusCode}).";

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
            }
            catch (JsonException)
            {
                // Keep the status phrase.
            }
        }

        return new ApiException(message, response.StatusCode);
    }
}
