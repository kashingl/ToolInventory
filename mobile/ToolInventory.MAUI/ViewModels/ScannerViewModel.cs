using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json;
using ToolInventory.MAUI.Navigation;
using ToolInventory.MAUI.Services;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.MAUI.ViewModels;

public partial class ScannerViewModel(IToolApiService apiService, IUserDialogService dialogService, IAuthService authService) : ObservableObject
{
    [ObservableProperty] public partial ToolDto? FoundTool { get; set; }
    [ObservableProperty] public partial string StatusMessage { get; set; } = "Point camera at a barcode or QR code";
    [ObservableProperty] public partial bool IsScanning { get; set; } = true;

    private string? _lastCode;
    private int _scanVersion;
    private bool _actionInProgress;

    public async Task OnBarcodeDetectedAsync(string code)
    {
        var toolId = ExtractToolId(code);
        if (!IsScanning || toolId == _lastCode)
        {
            return;
        }

        var currentScanVersion = Interlocked.Increment(ref _scanVersion);
        _lastCode = toolId;
        IsScanning = false;
        StatusMessage = $"Looking up: {toolId}";

        var result = await apiService.GetToolByBarcodeAsync(toolId);
        if (currentScanVersion != _scanVersion)
        {
            return;
        }

        if (result.IsSuccess && result.Value is not null)
        {
            FoundTool = result.Value;
            StatusMessage = string.Empty;
        }
        else
        {
            StatusMessage = result.StatusCode == 404
                ? $"No tool found for: {toolId}"
                : result.ErrorMessage ?? "Failed to look up barcode.";
        }
    }

    [RelayCommand]
    private void ScanAgain()
    {
        FoundTool = null;
        StatusMessage = "Point camera at a barcode or QR code";
        _lastCode = null;
        IsScanning = true;
    }

    [RelayCommand]
    private async Task CheckOutAsync()
    {
        if (FoundTool is null || _actionInProgress)
        {
            return;
        }

        var userId = await dialogService.PromptAsync(
            "Check Out",
            $"Check out \"{FoundTool.Name}\"?\nEnter user ID:",
            initialValue: authService.CurrentUserId);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        _actionInProgress = true;
        ApiResult<CheckoutDto> result;
        try
        {
            result = await apiService.CreateCheckoutAsync(new CreateCheckoutDto { ToolId = FoundTool.Id, UserId = userId.Trim() });
        }
        finally
        {
            _actionInProgress = false;
        }

        if (result.IsSuccess)
        {
            await dialogService.ShowAlertAsync("Success", $"\"{FoundTool.Name}\" checked out.");
            await dialogService.NavigateAsync(AppRoutes.Back);
        }
        else
        {
            await dialogService.ShowAlertAsync("Error", result.ErrorMessage ?? "Failed to check out tool.");
        }
    }

    [RelayCommand]
    private async Task CheckInAsync()
    {
        if (FoundTool is null || _actionInProgress)
        {
            return;
        }

        _actionInProgress = true;
        ApiResult result;
        try
        {
            result = await apiService.CheckInByToolIdAsync(FoundTool.Id);
        }
        finally
        {
            _actionInProgress = false;
        }

        if (result.IsSuccess)
        {
            await dialogService.ShowAlertAsync("Success", $"\"{FoundTool.Name}\" checked in.");
            await dialogService.NavigateAsync(AppRoutes.Back);
        }
        else
        {
            await dialogService.ShowAlertAsync("Info", result.ErrorMessage ?? "No active checkout found.");
        }
    }

    private static string ExtractToolId(string code)
    {
        var scanned = code.Trim();
        if (!scanned.StartsWith('{'))
        {
            return scanned;
        }

        try
        {
            using var document = JsonDocument.Parse(scanned);
            if (document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("id", out var idProperty)
                && idProperty.ValueKind == JsonValueKind.String)
            {
                var id = idProperty.GetString()?.Trim();
                if (!string.IsNullOrWhiteSpace(id))
                {
                    return id;
                }
            }
        }
        catch
        {
            // Keep backwards compatibility with plain barcode values.
        }

        return scanned;
    }
}
