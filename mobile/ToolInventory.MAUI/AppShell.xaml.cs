using ToolInventory.MAUI.Services;
using ToolInventory.MAUI.Views;

namespace ToolInventory.MAUI;

public partial class AppShell : Shell
{
    public AppShell(IAuthService authService)
    {
        InitializeComponent();
        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("tooldetail", typeof(ToolDetailPage));
        Routing.RegisterRoute("scanner", typeof(ScannerPage));

        Dispatcher.Dispatch(async () =>
        {
            await GoToAsync(authService.IsAuthenticated ? "//main/toolstab/tools" : "//login");
        });
    }
}
