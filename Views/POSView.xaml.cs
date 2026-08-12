using System.Windows.Controls;
using System.Windows.Input;
using SmartPOS.ViewModels;
namespace SmartPOS.Views;
public partial class POSView : UserControl
{
    public POSView() => InitializeComponent();
    private void BarcodeTextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || DataContext is not POSViewModel viewModel || !viewModel.AddByBarcodeCommand.CanExecute(null)) return;
        viewModel.AddByBarcodeCommand.Execute(null);
        e.Handled = true;
    }

    private async void POSView_OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is POSViewModel viewModel) await viewModel.StopCameraAsync();
    }
}
