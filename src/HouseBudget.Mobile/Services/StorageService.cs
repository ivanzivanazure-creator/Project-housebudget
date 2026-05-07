namespace HouseBudget.Mobile.Services;

public sealed class StorageService
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string UserIdKey = "user_id";
    private const string UserNameKey = "user_name";
    private const string CurrencyKey = "currency";

    public Task<string?> GetAccessTokenAsync() => SecureStorage.GetAsync(AccessTokenKey);
    public Task<string?> GetRefreshTokenAsync() => SecureStorage.GetAsync(RefreshTokenKey);
    public Task<string?> GetUserIdAsync() => SecureStorage.GetAsync(UserIdKey);
    public Task<string?> GetUserNameAsync() => SecureStorage.GetAsync(UserNameKey);
    public Task<string?> GetCurrencyAsync() => SecureStorage.GetAsync(CurrencyKey);

    public async Task SaveAuthDataAsync(string accessToken, string refreshToken, string userId, string userName, string currency)
    {
        await SecureStorage.SetAsync(AccessTokenKey, accessToken);
        await SecureStorage.SetAsync(RefreshTokenKey, refreshToken);
        await SecureStorage.SetAsync(UserIdKey, userId);
        await SecureStorage.SetAsync(UserNameKey, userName);
        await SecureStorage.SetAsync(CurrencyKey, currency);
    }

    public void ClearAuthData()
    {
        SecureStorage.Remove(AccessTokenKey);
        SecureStorage.Remove(RefreshTokenKey);
        SecureStorage.Remove(UserIdKey);
        SecureStorage.Remove(UserNameKey);
        SecureStorage.Remove(CurrencyKey);
    }

    public async Task<bool> IsLoggedInAsync()
    {
        var token = await GetAccessTokenAsync();
        return !string.IsNullOrEmpty(token);
    }
}
