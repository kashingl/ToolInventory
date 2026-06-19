using ToolInventory.MAUI.Services;
using ToolInventory.MAUI.ViewModels;

namespace ToolInventory.MAUI.Views;

public partial class ToolDetailPage : ContentPage
{
    public ToolDetailPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<ToolDetailViewModel>();
    }
}
