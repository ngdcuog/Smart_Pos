using System.Windows;
using SmartPOS.ViewModels;

namespace SmartPOS;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
