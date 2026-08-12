using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SmartPOS.ViewModels;
namespace SmartPOS.Views;
public partial class AttendanceView : UserControl
{
    public AttendanceView() => InitializeComponent();
    private void OnLoaded(object sender, RoutedEventArgs e) => Keyboard.Focus(EmployeeCodeTextBox);
    private void OnUnloaded(object sender, RoutedEventArgs e) { if (DataContext is AttendanceViewModel vm) _ = vm.StopCameraCommand.ExecuteAsync(null); }
}
