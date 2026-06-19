using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolInventory.MAUI.Navigation;
using ToolInventory.MAUI.Services;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.MAUI.ViewModels;

public partial class ToolDetailViewModel(IToolApiService apiService, IUserDialogService dialogService) : ObservableObject, IQueryAttributable
{
    [ObservableProperty] public partial ToolDto? Tool { get; set; }
    [ObservableProperty] public partial bool IsLoading { get; set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Tool", out var toolValue) && toolValue is ToolDto toolDto)
        {
            Tool = toolDto;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Tool is null)
        {
            return;
        }

        var confirm = await dialogService.ConfirmAsync("Delete", $"Delete \"{Tool.Name}\"?", "Delete", "Cancel");
        if (!confirm)
        {
            return;
        }

        IsLoading = true;
        try
        {
            var result = await apiService.DeleteToolAsync(Tool.Id);
            if (result.IsSuccess)
            {
                await dialogService.NavigateAsync(AppRoutes.Back);
            }
            else
            {
                await dialogService.ShowAlertAsync("Error", result.ErrorMessage ?? "Failed to delete tool.");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private Task GoBackAsync() => dialogService.NavigateAsync(AppRoutes.Back);
}
