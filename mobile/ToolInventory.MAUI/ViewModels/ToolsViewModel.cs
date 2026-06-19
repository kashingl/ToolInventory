using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolInventory.MAUI.Navigation;
using ToolInventory.MAUI.Services;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.MAUI.ViewModels;

public partial class ToolsViewModel(IToolApiService apiService, IUserDialogService dialogService) : ObservableObject
{
    [ObservableProperty] public partial ObservableCollection<ToolDto> Tools { get; set; } = [];
    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial string SearchText { get; set; } = string.Empty;

    private List<ToolDto> _allTools = [];

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
            var result = await apiService.GetToolsAsync();
            if (!result.IsSuccess)
            {
                await dialogService.ShowAlertAsync("Error", result.ErrorMessage ?? "Failed to load tools.");
                _allTools = [];
                ApplyFilter();
                return;
            }

            _allTools = result.Value ?? [];
            ApplyFilter();
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allTools
            : _allTools.Where(t =>
                t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || (t.Location?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
                || (t.Systainer?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
                || (t.Barcode?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();

        Tools = new ObservableCollection<ToolDto>(filtered);
    }

    [RelayCommand]
    private Task SelectToolAsync(ToolDto tool) =>
        dialogService.NavigateAsync(AppRoutes.ToolDetail, new Dictionary<string, object> { ["Tool"] = tool });

    [RelayCommand]
    private Task OpenScannerAsync() => dialogService.NavigateAsync(AppRoutes.Scanner);
}
