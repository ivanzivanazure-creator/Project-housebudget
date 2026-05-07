namespace HouseBudget.Mobile.Services;

public sealed class AuthService
{
    private readonly ApiService _apiService;
    private readonly StorageService _storage;

    public AuthService(ApiService apiService, StorageService storage)
    {
        _apiService = apiService;
        _storage = storage;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var result = await _apiService.PostAsync<AuthResponse>("api/v1/auth/login", new { email, password });
            if (result is null) return false;
            await _storage.SaveAuthDataAsync(result.AccessToken, result.RefreshToken, result.UserId.ToString(), result.FullName, result.DefaultCurrency);
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> RegisterAsync(string firstName, string lastName, string email, string password, string currency = "USD")
    {
        try
        {
            var result = await _apiService.PostAsync<AuthResponse>("api/v1/auth/register", new { firstName, lastName, email, password, currency });
            if (result is null) return false;
            await _storage.SaveAuthDataAsync(result.AccessToken, result.RefreshToken, result.UserId.ToString(), result.FullName, result.DefaultCurrency);
            return true;
        }
        catch { return false; }
    }

    public void Logout() => _storage.ClearAuthData();

    public Task<bool> IsLoggedInAsync() => _storage.IsLoggedInAsync();
}

public record AuthResponse(Guid UserId, string Email, string FullName, string AccessToken, string RefreshToken, DateTime ExpiresAt, string DefaultCurrency);
