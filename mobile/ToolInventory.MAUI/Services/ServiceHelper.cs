using Microsoft.Extensions.DependencyInjection;

namespace ToolInventory.MAUI.Services;

public static class ServiceHelper
{
    public static T GetService<T>() where T : class
    {
        var services = IPlatformApplication.Current?.Services
            ?? throw new InvalidOperationException("Unable to access the application service provider.");

        return services.GetRequiredService<T>();
    }
}
