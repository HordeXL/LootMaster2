using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LootMaster.ViewModels;

namespace LootMaster;

public partial class MainWindow : Window
{
    private MainViewModel _vm = null!;

    public MainWindow()
    {
        InitializeComponent();
        _vm = new MainViewModel();
        DataContext = _vm;

        _vm.ScrollIntoViewRequested += row =>
        {
            ItemsGrid.ScrollIntoView(row);
            ItemsGrid.Focus();
        };

        Loaded += (_, _) => ItemsGrid.Focus();

        NpcDetailGrid.SelectionChanged += OnNpcDetailSelectionChanged;
        NpcDetailGrid.MouseDoubleClick += OnNpcDetailDoubleClick;
        NpcListBox.SelectionChanged += (_, _) => ItemsGrid.Focus();
    }

    private void OnNpcDetailSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Single click: just focus the main grid
        ItemsGrid.Focus();
    }

    private void OnNpcDetailDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // Double click: jump to item in main grid
        if (_vm.JumpToNpcItemCommand.CanExecute(null))
            _vm.JumpToNpcItemCommand.Execute(null);
    }
}
