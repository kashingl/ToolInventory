using ToolInventory.MAUI.Services;
using ToolInventory.MAUI.ViewModels;
using ZXing.Net.Maui;

namespace ToolInventory.MAUI.Views;

public partial class ScannerPage : ContentPage
{
    private readonly ScannerViewModel _vm;

    public ScannerPage()
    {
        InitializeComponent();
        BindingContext = _vm = ServiceHelper.GetService<ScannerViewModel>();
    }

    private async void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var first = e.Results.FirstOrDefault();
        if (first is null)
        {
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(() => _vm.OnBarcodeDetectedAsync(first.Value));
    }
}
