namespace ToolInventory.MAUI.Services;

public sealed class ApiConfiguration : IApiConfiguration
{
    public const string BaseUrlPreferenceKey = "api_base_url";

    public string BaseUrl => Preferences.Default.Get(BaseUrlPreferenceKey, DeviceInfo.Platform == DevicePlatform.Android
        ? "http://10.0.2.2:5177"
        : "http://localhost:5177");
}
