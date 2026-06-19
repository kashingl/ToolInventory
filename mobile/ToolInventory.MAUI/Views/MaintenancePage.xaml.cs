using ToolInventory.MAUI.Services;
using ToolInventory.MAUI.ViewModels;

namespace ToolInventory.MAUI.Views;

public partial class MaintenancePage : ContentPage
{
    private readonly MaintenanceViewModel _vm;

    public MaintenancePage()
    {
        InitializeComponent();
        BindingContext = _vm = ServiceHelper.GetService<MaintenanceViewModel>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadCommand.Execute(null);
    }
}
