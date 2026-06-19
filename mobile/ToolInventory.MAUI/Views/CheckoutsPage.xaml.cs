using ToolInventory.MAUI.Services;
using ToolInventory.MAUI.ViewModels;

namespace ToolInventory.MAUI.Views;

public partial class CheckoutsPage : ContentPage
{
    private readonly CheckoutsViewModel _vm;

    public CheckoutsPage()
    {
        InitializeComponent();
        BindingContext = _vm = ServiceHelper.GetService<CheckoutsViewModel>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadCommand.Execute(null);
    }
}
