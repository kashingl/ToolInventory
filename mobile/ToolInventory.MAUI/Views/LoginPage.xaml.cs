using ToolInventory.MAUI.Services;
using ToolInventory.MAUI.ViewModels;

namespace ToolInventory.MAUI.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<LoginViewModel>();
    }
}
