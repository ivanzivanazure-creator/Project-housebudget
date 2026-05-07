namespace HouseBudget.Mobile;

public static class AppConfig
{
#if DEBUG
    public const string ApiBaseUrl = "https://10.0.2.2:7001/"; // Android emulator localhost
#else
    public const string ApiBaseUrl = "https://api.housebudget.com/";
#endif
}
