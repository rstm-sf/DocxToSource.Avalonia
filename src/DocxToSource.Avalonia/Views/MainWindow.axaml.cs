using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using DocxToSource.Avalonia.ViewModels;

namespace DocxToSource.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetRequiredService<MainWindowViewModel>();
    }
}
