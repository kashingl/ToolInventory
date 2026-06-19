using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolInventory.MAUI.Navigation;
using ToolInventory.MAUI.Services;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.MAUI.ViewModels;

public partial class LoginViewModel(IAuthService authService, IUserDialogService dialogService) : ObservableObject
{
    [ObservableProperty] public partial string Email { get; set; } = string.Empty;
    [ObservableProperty] public partial string Password { get; set; } = string.Empty;
    [ObservableProperty] public partial string ErrorMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsLoading { get; set; }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email and password are required.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await authService.LoginAsync(new LoginDto { Email = Email, Password = Password });
            if (result.IsSuccess)
            {
                await dialogService.NavigateAsync(AppRoutes.MainTools);
                return;
            }

            ErrorMessage = result.ErrorMessage ?? "Invalid email or password.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private Task GoToRegisterAsync() => dialogService.NavigateAsync(AppRoutes.Register);
}
