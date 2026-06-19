using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolInventory.MAUI.Services;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.MAUI.ViewModels;

public partial class CheckoutsViewModel(IToolApiService apiService, IUserDialogService dialogService, IAuthService authService) : ObservableObject
{
    [ObservableProperty] public partial ObservableCollection<CheckoutDto> Checkouts { get; set; } = [];
    [ObservableProperty] public partial bool IsLoading { get; set; }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        try
        {
            var result = await apiService.GetCheckoutsAsync();
            if (!result.IsSuccess)
            {
                await dialogService.ShowAlertAsync("Error", result.ErrorMessage ?? "Failed to load checkouts.");
                Checkouts = [];
                return;
            }

            Checkouts = new ObservableCollection<CheckoutDto>(result.Value ?? []);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CheckInAsync(CheckoutDto checkout)
    {
        var confirm = await dialogService.ConfirmAsync("Check In", $"Check in \"{checkout.ToolName}\"?", "Yes", "No");
        if (!confirm)
        {
            return;
        }

        IsLoading = true;
        try
        {
            var result = await apiService.CheckInAsync(checkout.Id);
            if (result.IsSuccess)
            {
                await LoadAsync();
            }
            else
            {
                await dialogService.ShowAlertAsync("Error", result.ErrorMessage ?? "Failed to check in tool.");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CheckOutToolAsync()
    {
        var availableResult = await apiService.GetToolsAsync("Available");
        if (!availableResult.IsSuccess)
        {
            await dialogService.ShowAlertAsync("Error", availableResult.ErrorMessage ?? "Failed to load available tools.");
            return;
        }

        var available = availableResult.Value ?? [];
        if (available.Count == 0)
        {
            await dialogService.ShowAlertAsync("Info", "No tools available.");
            return;
        }

        var toolName = await dialogService.ActionSheetAsync("Select Tool", "Cancel", available.Select(t => t.Name).ToArray());
        if (string.IsNullOrWhiteSpace(toolName) || toolName == "Cancel")
        {
            return;
        }

        var tool = available.First(t => t.Name == toolName);
        var userId = await dialogService.PromptAsync(
            "User ID",
            "Enter your user ID:",
            initialValue: authService.CurrentUserId);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        IsLoading = true;
        try
        {
            var result = await apiService.CreateCheckoutAsync(new CreateCheckoutDto { ToolId = tool.Id, UserId = userId.Trim() });
            if (result.IsSuccess)
            {
                await LoadAsync();
            }
            else
            {
                await dialogService.ShowAlertAsync("Error", result.ErrorMessage ?? "Failed to check out tool.");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
