using System.Windows;
using LootMaster.ViewModels;

namespace LootMaster;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var vm = new MainViewModel();
        DataContext = vm;
        vm.ScrollIntoViewRequested += row =>
        {
            ItemsGrid.ScrollIntoView(row);
            ItemsGrid.Focus();
        };
    }
}
