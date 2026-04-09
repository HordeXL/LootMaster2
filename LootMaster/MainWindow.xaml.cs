using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LootMaster.Helpers;
using LootMaster.Services;
using LootMaster.ViewModels;

namespace LootMaster;

public partial class MainWindow : Window
{
    private MainViewModel _vm = null!;
    private readonly ColumnSettingsService _colSettings = new(
        Path.Combine(AppContext.BaseDirectory, "Data", "column-settings.json"));

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

        _vm.LanguageChanged += () =>
        {
            ApplyColumnHeaders(_vm.UI);
            _colSettings.Save(ItemsGrid, MainGridLeft, this);
        };

        Loaded += (_, _) =>
        {
            _colSettings.Restore(ItemsGrid, MainGridLeft, this);
            ApplyColumnHeaders(_vm.UI);
            ItemsGrid.Focus();
        };

        Closing += async (_, _) =>
        {
            _colSettings.Save(ItemsGrid, MainGridLeft, this);
            await _vm.SaveOnCloseAsync();
        };

        ItemsGrid.ColumnReordered += (_, _) => _colSettings.Save(ItemsGrid, MainGridLeft, this);

        NpcDetailGrid.SelectionChanged += OnNpcDetailSelectionChanged;
        NpcDetailGrid.MouseDoubleClick += OnNpcDetailDoubleClick;
        NpcListBox.SelectionChanged += (_, _) => ItemsGrid.Focus();
    }

    /// <summary>
    /// Updates DataGrid column headers to match the current language.
    /// Column persistence uses binding path as stable key, so headers can be changed freely.
    /// </summary>
    private void ApplyColumnHeaders(UIStrings ui)
    {
        SetColumnHeader(ItemsGrid,    "ItemName",      ui.ColItem);
        SetColumnHeader(ItemsGrid,    "CategoryName",  ui.ColCategory);
        SetColumnHeader(ItemsGrid,    "ItemGroup",     ui.ColGroupItem);
        SetColumnHeader(ItemsGrid,    "ItemChance",    ui.ColChanceItem);
        SetColumnHeader(ItemsGrid,    "CategoryGroup", ui.ColGroupCat);
        SetColumnHeader(ItemsGrid,    "CategoryChance",ui.ColChanceCat);
        SetColumnHeader(ItemsGrid,    "EffectiveGroup",ui.ColGroupEff);
        SetColumnHeader(ItemsGrid,    "EffectiveChance",ui.ColChanceEff);
        SetColumnHeader(ItemsGrid,    "DbGroup",       ui.ColGroupDb);
        SetColumnHeader(ItemsGrid,    "DbChance",      ui.ColChanceDb);
        SetColumnHeader(ItemsGrid,    "ItemMinAmount",       ui.ColMinAmount);
        SetColumnHeader(ItemsGrid,    "ItemMaxAmount",       ui.ColMaxAmount);
        SetColumnHeader(ItemsGrid,    "EffectiveGradeId",    ui.ColGradeId);
        SetColumnHeader(ItemsGrid,    "EffectiveAlwaysDrop", ui.ColAlwaysDrop);

        SetColumnHeader(NpcDetailGrid,"ItemName",      ui.ColItem);
        SetColumnHeader(NpcDetailGrid,"EffectiveGroup",ui.ColGroup);
        SetColumnHeader(NpcDetailGrid,"EffectiveChance",ui.ColChance);
    }

    private static void SetColumnHeader(DataGrid grid, string bindingPath, string header)
    {
        foreach (var col in grid.Columns)
        {
            if (col is DataGridBoundColumn bc &&
                bc.Binding is System.Windows.Data.Binding b &&
                b.Path?.Path == bindingPath)
            {
                col.Header = header;
                return;
            }
        }
    }

    private void OnNpcDetailSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ItemsGrid.Focus();
    }

    private void OnNpcDetailDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_vm.JumpToNpcItemCommand.CanExecute(null))
            _vm.JumpToNpcItemCommand.Execute(null);
    }
}
