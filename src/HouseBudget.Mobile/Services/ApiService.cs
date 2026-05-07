using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HouseBudget.Mobile.Services;

public sealed class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly StorageService _storage;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public ApiService(StorageService storage)
    {
        _storage = storage;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(AppConfig.ApiBaseUrl)
        };
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _storage.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync(endpoint, ct);
        return await DeserializeResponseAsync<T>(response);
    }

    public async Task<T?> PostAsync<T>(string endpoint, object body, CancellationToken ct = default)
    {
        await SetAuthHeaderAsync();
        var json = JsonSerializer.Serialize(body, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, content, ct);
        return await DeserializeResponseAsync<T>(response);
    }

    public async Task<T?> PutAsync<T>(string endpoint, object body, CancellationToken ct = default)
    {
        await SetAuthHeaderAsync();
        var json = JsonSerializer.Serialize(body, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync(endpoint, content, ct);
        return await DeserializeResponseAsync<T>(response);
    }

    public async Task<bool> DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync(endpoint, ct);
        return response.IsSuccessStatusCode;
    }

    private async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"API Error {(int)response.StatusCode}: {json}");

        var wrapper = JsonSerializer.Deserialize<ApiResponse<T>>(json, JsonOptions);
        return wrapper is not null ? wrapper.Data : default;
    }
}

public record ApiResponse<T>(bool Success, string? Message, T? Data);
