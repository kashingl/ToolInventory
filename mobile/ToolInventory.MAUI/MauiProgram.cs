using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using ToolInventory.MAUI.Services;
using ToolInventory.MAUI.ViewModels;
using ZXing.Net.Maui.Controls;

namespace ToolInventory.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBarcodeReader()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<IApiConfiguration, ApiConfiguration>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IUserDialogService, UserDialogService>();
        builder.Services.AddTransient<AuthMessageHandler>();
        
        builder.Services.AddSingleton(sp =>
        {
            var apiConfig = sp.GetRequiredService<IApiConfiguration>();
            var handler = sp.GetRequiredService<AuthMessageHandler>();
            handler.InnerHandler = new HttpClientHandler();
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(apiConfig.BaseUrl)
            };
            return client;
        });
        
        builder.Services.AddSingleton<IToolApiService>(sp =>
            new ToolApiService(
                sp.GetRequiredService<HttpClient>(),
                sp.GetRequiredService<IApiConfiguration>()));
        
        builder.Services.AddSingleton<AppShell>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ToolsViewModel>();
        builder.Services.AddTransient<ToolDetailViewModel>();
        builder.Services.AddTransient<CheckoutsViewModel>();
        builder.Services.AddTransient<MaintenanceViewModel>();
        builder.Services.AddTransient<ScannerViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
