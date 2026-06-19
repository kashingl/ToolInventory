using ToolInventory.MAUI.Services;
using ToolInventory.MAUI.ViewModels;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.MAUI.Views;

public partial class ToolsPage : ContentPage
{
    private readonly ToolsViewModel _vm;

    public ToolsPage()
    {
        InitializeComponent();
        BindingContext = _vm = ServiceHelper.GetService<ToolsViewModel>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadCommand.Execute(null);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ToolDto tool)
        {
            _vm.SelectToolCommand.Execute(tool);
        }

        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }
    }
}
