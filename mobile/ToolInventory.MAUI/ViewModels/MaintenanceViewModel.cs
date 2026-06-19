using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolInventory.MAUI.Services;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.MAUI.ViewModels;

public partial class MaintenanceViewModel(IToolApiService apiService, IUserDialogService dialogService) : ObservableObject
{
    [ObservableProperty] public partial ObservableCollection<MaintenanceRecordDto> Records { get; set; } = [];
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
            var result = await apiService.GetMaintenanceRecordsAsync();
            if (!result.IsSuccess)
            {
                await dialogService.ShowAlertAsync("Error", result.ErrorMessage ?? "Failed to load maintenance records.");
                Records = [];
                return;
            }

            Records = new ObservableCollection<MaintenanceRecordDto>(result.Value ?? []);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddRecordAsync()
    {
        var toolsResult = await apiService.GetToolsAsync();
        if (!toolsResult.IsSuccess)
        {
            await dialogService.ShowAlertAsync("Error", toolsResult.ErrorMessage ?? "Failed to load tools.");
            return;
        }

        var tools = toolsResult.Value ?? [];
        var toolNames = tools.Select(t => t.Name).ToArray();
        if (toolNames.Length == 0)
        {
            await dialogService.ShowAlertAsync("Info", "No tools found.");
            return;
        }

        var toolName = await dialogService.ActionSheetAsync("Select Tool", "Cancel", toolNames);
        if (string.IsNullOrWhiteSpace(toolName) || toolName == "Cancel")
        {
            return;
        }

        var tool = tools.First(t => t.Name == toolName);
        var description = await dialogService.PromptAsync("Description", "Describe the maintenance work:");
        if (string.IsNullOrWhiteSpace(description))
        {
            return;
        }

        var performedBy = await dialogService.PromptAsync("Performed By", "Who performed this? (optional):");

        IsLoading = true;
        try
        {
            var result = await apiService.CreateMaintenanceRecordAsync(new CreateMaintenanceRecordDto
            {
                ToolId = tool.Id,
                Date = DateTime.UtcNow,
                Description = description,
                PerformedBy = string.IsNullOrWhiteSpace(performedBy) ? null : performedBy
            });

            if (result.IsSuccess)
            {
                await LoadAsync();
            }
            else
            {
                await dialogService.ShowAlertAsync("Error", result.ErrorMessage ?? "Failed to create record.");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
