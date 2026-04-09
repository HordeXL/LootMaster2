using LootMaster.Helpers;
using LootMaster.Models;
using LootMaster.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace LootMaster.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    public event Action<ItemRow>? ScrollIntoViewRequested;

    // ──────────────────────────────────────────────────────────────────────────
    // State
    // ──────────────────────────────────────────────────────────────────────────

    private AppProgress _progress = new();
    private readonly ProgressService _progressSvc;
    private readonly ColumnSettingsService _colSvc;
    private bool _preferRussian = true;
    private UIStrings _ui = new(true);

    // All item rows (source)
    private readonly ObservableCollection<ItemRow> _allRows = new();

    // Filtered view (shown in DataGrid)
    public ICollectionView ItemsView { get; }

    // NPC data for the item currently selected
    public ObservableCollection<string> NpcListItems { get; }
    public ObservableCollection<NpcItemRow> NpcDetailItems { get; }

    // Raw lookup maps (set after loading)
    private Dictionary<int, ItemInfo> _itemsData = new();
    private Dictionary<int, HashSet<int>> _npcToItems = new();
    private Dictionary<int, string> _npcNames = new();

    private ItemRow? _selectedItem;
    private string? _selectedNpcText;
    private int _selectedNpcId;

    // Column visibility
    private bool _showNpcIdColumn = true;
    private bool _showNpcNameColumn = true;

    // Editor fields
    private string _itemGroupText = "";
    private string _itemChanceText = "";
    private string _categoryGroupText = "";
    private string _categoryChanceText = "";

    // Filter
    private string _searchText = "";
    private bool _onlyUnprocessed;
    private bool _highlightCategoryGroup = true;

    // Status
    private string _statusText = "";
    private bool _isLoading;

    private CancellationTokenSource? _loadCts;

    // ──────────────────────────────────────────────────────────────────────────
    // Constructor
    // ──────────────────────────────────────────────────────────────────────────

    public MainViewModel()
    {
        NpcListItems = new ObservableCollection<string>();
        NpcDetailItems = new ObservableCollection<NpcItemRow>();
        NpcListItems.CollectionChanged   += (_, _) => OnPropertyChanged(nameof(NpcListLabelText));
        NpcDetailItems.CollectionChanged += (_, _) => OnPropertyChanged(nameof(NpcDetailLabelText));

        string stateFile = Path.Combine(
            AppContext.BaseDirectory,
            "Data", "loot_group_progress.json");
        _progressSvc = new ProgressService(stateFile);

        string colFile = Path.Combine(AppContext.BaseDirectory, "Data", "column-settings.json");
        _colSvc = new ColumnSettingsService(colFile);
        _preferRussian = _colSvc.LoadPreferRussian();
        _ui = new UIStrings(_preferRussian);
        StatusText = _ui.InitialStatus;

        ItemsView = CollectionViewSource.GetDefaultView(_allRows);
        ItemsView.Filter = FilterRow;

        // Commands
        OpenDbCommand = new RelayCommand(async () => await OpenDbAsync());
        OpenLootDbCommand = new RelayCommand(async () => await OpenLootDbAsync());
        OpenJsonCommand = new RelayCommand(async () => await OpenJsonAsync());
        SaveProgressCommand = new RelayCommand(async () => await SaveProgressAsync());
        ExportCommand = new RelayCommand(async () => await ExportAsync(), () => _allRows.Count > 0);
        SyncDbCommand = new RelayCommand(async () => await SyncDbAsync(), () => _allRows.Count > 0);
        ImportSqlCommand = new RelayCommand(async () => await ImportSqlAsync(), () => !string.IsNullOrEmpty(EffectiveLootDbPath));
        ApplyItemCommand = new RelayCommand(ApplyItemSettings, () => _selectedItem is not null);
        ClearItemCommand = new RelayCommand(ClearItemSettings, () => _selectedItem is not null);
        ApplyAllFromDbCommand = new RelayCommand(ApplyAllFromDb, () => _allRows.Any(r => r.DbGroup.HasValue && r.DbChance.HasValue));
        ShowGroupHelpCommand = new RelayCommand(ShowGroupHelp);
        ApplyCategoryCommand = new RelayCommand(ApplyCategorySettings, () => _selectedItem is not null);
        ClearCategoryCommand = new RelayCommand(ClearCategorySettings, () => _selectedItem is not null);
        PrevCommand = new RelayCommand(SelectPrev);
        NextCommand = new RelayCommand(SelectNext);
        NextUnprocessedCommand = new RelayCommand(SelectNextUnprocessed);
        JumpToNpcItemCommand = new RelayCommand(JumpToSelectedNpcItem);
        ToggleLanguageCommand = new RelayCommand(ToggleLanguage);

        _ = RestoreSessionAsync();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Commands
    // ──────────────────────────────────────────────────────────────────────────

    public ICommand OpenDbCommand { get; }
    public ICommand OpenLootDbCommand { get; }
    public ICommand OpenJsonCommand { get; }
    public ICommand SaveProgressCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand SyncDbCommand { get; }
    public ICommand ImportSqlCommand { get; }
    public ICommand ApplyItemCommand { get; }
    public ICommand ClearItemCommand { get; }
    public ICommand ApplyAllFromDbCommand { get; }
    public ICommand ShowGroupHelpCommand { get; }
    public ICommand ApplyCategoryCommand { get; }
    public ICommand ClearCategoryCommand { get; }
    public ICommand PrevCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand NextUnprocessedCommand { get; }
    public ICommand JumpToNpcItemCommand { get; }
    public ICommand ToggleLanguageCommand { get; }

    // ──────────────────────────────────────────────────────────────────────────
    // Bindable properties
    // ──────────────────────────────────────────────────────────────────────────

    public string StatusText { get => _statusText; set => Set(ref _statusText, value); }
    public bool IsLoading { get => _isLoading; set => Set(ref _isLoading, value); }
    public string SearchText { get => _searchText; set { Set(ref _searchText, value); ItemsView.Refresh(); } }
    public bool OnlyUnprocessed { get => _onlyUnprocessed; set { Set(ref _onlyUnprocessed, value); ItemsView.Refresh(); RefreshNpcPanel(); } }
    public bool HighlightCategoryGroup { get => _highlightCategoryGroup; set => Set(ref _highlightCategoryGroup, value); }
    public bool ShowNpcIdColumn { get => _showNpcIdColumn; set => Set(ref _showNpcIdColumn, value); }
    public bool ShowNpcNameColumn { get => _showNpcNameColumn; set => Set(ref _showNpcNameColumn, value); }
    public UIStrings UI { get => _ui; private set => Set(ref _ui, value); }
    public bool PreferRussian { get => _preferRussian; set => Set(ref _preferRussian, value); }
    public string LanguageLabel => _preferRussian ? "RU" : "EN";

    public ItemRow? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (Set(ref _selectedItem, value))
            {
                OnItemSelected(value);
                OnPropertyChanged(nameof(SelectedItemInfoText));
            }
        }
    }

    public string? SelectedNpcText
    {
        get => _selectedNpcText;
        set
        {
            if (Set(ref _selectedNpcText, value))
                OnNpcSelected(value);
        }
    }

    public string ItemGroupText { get => _itemGroupText; set => Set(ref _itemGroupText, value); }
    public string ItemChanceText { get => _itemChanceText; set => Set(ref _itemChanceText, value); }
    public string CategoryGroupText { get => _categoryGroupText; set => Set(ref _categoryGroupText, value); }
    public string CategoryChanceText { get => _categoryChanceText; set => Set(ref _categoryChanceText, value); }

    public ObservableCollection<string> LoadedJsonFiles { get; } = new();

    // Translated labels for NPC browser and item info box
    public string NpcListLabelText   => _ui.NpcListLabel(NpcListItems.Count);
    public string NpcDetailLabelText => _ui.NpcDetailLabel(NpcDetailItems.Count);
    public string SelectedItemInfoText =>
        _selectedItem is null ? "" :
        string.Format(_ui.InfoFormat,
            _selectedItem.ItemId, _selectedItem.ItemName,
            _selectedItem.CategoryId, _selectedItem.CategoryName);

    // Summary text shown in the summary panel
    private string _summaryLeft = "";
    private string _summaryRight = "";
    // key = filename, value = cumulative totals for that file
    private readonly Dictionary<string, (int Inserted, int Updated)> _sqlImports = [];
    public string SummaryLeft  { get => _summaryLeft;  private set => Set(ref _summaryLeft,  value); }
    public string SummaryRight { get => _summaryRight; private set => Set(ref _summaryRight, value); }

    // ──────────────────────────────────────────────────────────────────────────
    // Loading
    // ──────────────────────────────────────────────────────────────────────────

    private async Task RestoreSessionAsync()
    {
        var saved = await _progressSvc.LoadAsync();
        if (saved is null) return;
        _progress = saved;

        // Удаляем из сохранённого списка файлы, которых больше нет на диске
        var missing = _progress.SourceJsonPaths.Where(p => !File.Exists(p)).ToList();
        foreach (var p in missing) _progress.SourceJsonPaths.Remove(p);

        if (!string.IsNullOrEmpty(_progress.DbPath)
            && File.Exists(_progress.DbPath)
            && _progress.SourceJsonPaths.Count > 0)
        {
            await LoadDataAsync(_progress.DbPath, _progress.SourceJsonPaths);
        }
    }

    /// <summary>Returns loot DB path if set, otherwise falls back to main DB path.</summary>
    private string EffectiveLootDbPath =>
        !string.IsNullOrEmpty(_progress.LootDbPath) ? _progress.LootDbPath : _progress.DbPath;

    private async Task OpenDbAsync()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Выбери SQLite базу (предметы, NPC, категории)",
            Filter = "SQLite (*.sqlite3;*.db)|*.sqlite3;*.db|Все файлы|*.*"
        };
        if (dlg.ShowDialog() != true) return;
        _progress.DbPath = dlg.FileName;
        StatusText = _ui.StatusDbOpened(Path.GetFileName(dlg.FileName));
        await TrySilentSave();
    }

    private async Task OpenLootDbAsync()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Выбери SQLite базу лута (loots, loot_groups, loot_pack_dropping_npcs)",
            Filter = "SQLite (*.sqlite3;*.db)|*.sqlite3;*.db|Все файлы|*.*"
        };
        if (dlg.ShowDialog() != true) return;
        _progress.LootDbPath = dlg.FileName;
        StatusText = _ui.StatusLootDbOpened(Path.GetFileName(dlg.FileName));
        await TrySilentSave();
    }

    private async Task OpenJsonAsync()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Выбери JSON файл(ы) дропа",
            Filter = "JSON (*.json)|*.json|Все файлы|*.*",
            Multiselect = true
        };
        if (dlg.ShowDialog() != true) return;

        if (string.IsNullOrEmpty(_progress.DbPath) || !File.Exists(_progress.DbPath))
        {
            MessageBox.Show("Сначала выбери SQLite базу.", "Нет базы", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        foreach (var f in dlg.FileNames)
            if (!_progress.SourceJsonPaths.Contains(f))
                _progress.SourceJsonPaths.Add(f);

        await LoadDataAsync(_progress.DbPath, _progress.SourceJsonPaths);
        await TrySilentSave();
    }

    private async Task LoadDataAsync(string dbPath, IReadOnlyList<string> jsonPaths)
    {
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var ct = _loadCts.Token;

        IsLoading = true;
        _allRows.Clear();
        NpcListItems.Clear();
        NpcDetailItems.Clear();
        LoadedJsonFiles.Clear();
        foreach (var p in jsonPaths) LoadedJsonFiles.Add(Path.GetFileName(p));

        try
        {
            var progress = new Progress<string>(msg => StatusText = msg);

            // 1. Parse JSON
            StatusText = _ui.StatusParsingJson;
            var parseResult = await JsonSourceParser.ParseAsync(jsonPaths);

            if (parseResult.SkippedFiles.Count > 0)
                MessageBox.Show(
                    $"Пропущены файлы (не является массивом JSON):\n{string.Join('\n', parseResult.SkippedFiles)}",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);

            var itemIds = parseResult.ItemToNpcs.Keys.OrderBy(x => x).ToList();
            var npcIds = parseResult.NpcToItems.Keys.OrderBy(x => x).ToList();

            StatusText = $"JSON: {itemIds.Count} предметов, {npcIds.Count} NPC. Загружаем имена…";

            var dbSvc = new DatabaseService(dbPath, EffectiveLootDbPath, _preferRussian);

            // 2. Load NPC names first (needed by LoadItemsAsync)
            _npcNames = await dbSvc.LoadNpcNamesAsync(npcIds, progress, ct);

            // 3. Load items + categories
            _itemsData = await dbSvc.LoadItemsAsync(itemIds, parseResult.ItemToNpcs, _npcNames, parseResult.DoodadToLootPack, progress, ct);
            _npcToItems = parseResult.NpcToItems;

            // Warn if loot tables were missing (all LootPackIds and DbGroup/DbChance will be empty)
            bool lootMissing = _itemsData.Values.All(i => i.LootPackIds.Count == 0 && i.DbGroup == null);
            if (lootMissing && _itemsData.Count > 0 && string.IsNullOrEmpty(_progress.LootDbPath))
                MessageBox.Show(
                    "Лут-таблицы (loots, loot_pack_dropping_npcs) не найдены в основной базе.\n\n" +
                    "Если они находятся в отдельном файле (например compact.server.table.sqlite3),\n" +
                    "нажми кнопку «Открыть БД лута» на панели инструментов.",
                    "Нет данных лута", MessageBoxButton.OK, MessageBoxImage.Warning);

            // 4. Populate rows on UI thread
            foreach (var id in _itemsData.Keys.OrderBy(x => x))
            {
                var info = _itemsData[id];
                var ip = GetOrCreateItemProgress(id);
                var cp = GetOrCreateCategoryProgress(info.CategoryId);
                _allRows.Add(new ItemRow(info, ip, cp));
            }

            ItemsView.Refresh();
            RefreshSummary();

            // Select first visible
            var first = ItemsView.Cast<ItemRow>().FirstOrDefault();
            if (first is not null)
                SelectedItem = first;

            StatusText = _ui.StatusReady(_itemsData.Count, _npcNames.Count, _progress.Categories.Count);
        }
        catch (OperationCanceledException)
        {
            StatusText = _ui.StatusCancelled;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText = _ui.StatusLoadError;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Filter
    // ──────────────────────────────────────────────────────────────────────────

    private bool FilterRow(object obj)
    {
        if (obj is not ItemRow row) return false;

        if (_onlyUnprocessed && row.IsProcessed) return false;

        if (string.IsNullOrWhiteSpace(_searchText)) return true;

        var needle = _searchText.Trim().ToLowerInvariant();
        var haystack = $"{row.ItemId} {row.ItemName} {row.CategoryId} {row.CategoryName} {row.NpcIdsText} {row.NpcNamesText}"
                       .ToLowerInvariant();
        return haystack.Contains(needle);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Item selection
    // ──────────────────────────────────────────────────────────────────────────

    private void OnItemSelected(ItemRow? row)
    {
        if (row is null)
        {
            ClearEditor();
            NpcListItems.Clear();
            NpcDetailItems.Clear();
            return;
        }

        // Populate editor — prefer saved value, then DB value, then default
        ItemGroupText  = row.ItemGroup?.ToString()  ?? row.DbGroup?.ToString()  ?? "0";
        ItemChanceText = row.ItemChance?.ToString("0.######") ?? row.DbChance?.ToString("0.######") ?? "100.0";
        CategoryGroupText  = row.CategoryGroup?.ToString()  ?? "0";
        CategoryChanceText = row.CategoryChance?.ToString("0.######") ?? "100.0";

        RefreshNpcPanel();
    }

    private void RefreshNpcPanel()
    {
        if (_selectedItem is null)
        {
            NpcListItems.Clear();
            NpcDetailItems.Clear();
            return;
        }

        NpcListItems.Clear();
        NpcDetailItems.Clear();
        _selectedNpcId = 0;

        var npcIds = GetVisibleNpcIdsForItem(_selectedItem.ItemId);
        foreach (var npcId in npcIds)
        {
            var name = _npcNames.TryGetValue(npcId, out var n) ? n : "";
            var visible = GetVisibleItemIdsForNpc(npcId).Count;
            NpcListItems.Add(string.IsNullOrEmpty(name) ? $"{npcId}  [{visible}]" : $"{npcId} — {name}  [{visible}]");
        }

        if (npcIds.Count > 0)
        {
            _selectedNpcId = npcIds[0];
            PopulateNpcDetail(_selectedNpcId);
            OnPropertyChanged(nameof(SelectedNpcText));
        }
    }

    private void OnNpcSelected(string? text)
    {
        if (text is null || _selectedItem is null) return;
        int idx = NpcListItems.IndexOf(text);
        if (idx < 0) return;
        var npcIds = GetVisibleNpcIdsForItem(_selectedItem.ItemId);
        if (idx >= npcIds.Count) return;
        _selectedNpcId = npcIds[idx];
        PopulateNpcDetail(_selectedNpcId);
    }

    private void PopulateNpcDetail(int npcId)
    {
        NpcDetailItems.Clear();
        foreach (var itemId in GetVisibleItemIdsForNpc(npcId))
        {
            if (!_itemsData.TryGetValue(itemId, out var info)) continue;
            var ip = GetOrCreateItemProgress(itemId);
            var cp = GetOrCreateCategoryProgress(info.CategoryId);
            var tempRow = new ItemRow(info, ip, cp);

            // Find loot_pack_id for this specific NPC from the item's pack list
            int npcIndex = info.NpcIds.IndexOf(npcId);
            int? lootPackId = npcIndex >= 0 && npcIndex < info.LootPackIds.Count
                ? info.LootPackIds[npcIndex]
                : (int?)null;

            NpcDetailItems.Add(new NpcItemRow(
                itemId,
                info.ItemName,
                info.CategoryName,
                lootPackId,
                tempRow.EffectiveGroup,
                tempRow.EffectiveChance,
                tempRow.HighlightState));
        }
    }

    private List<int> GetVisibleNpcIdsForItem(int itemId)
    {
        if (!_itemsData.TryGetValue(itemId, out var info)) return [];
        var npcIds = info.NpcIds;
        if (!_onlyUnprocessed) return npcIds;

        return npcIds.Where(npcId =>
            _npcToItems.TryGetValue(npcId, out var items) &&
            items.Any(i => _itemsData.ContainsKey(i) && !GetOrCreateItemRow(i).IsProcessed)
        ).ToList();
    }

    private List<int> GetVisibleItemIdsForNpc(int npcId)
    {
        if (!_npcToItems.TryGetValue(npcId, out var items)) return [];
        var result = items.Where(_itemsData.ContainsKey).OrderBy(x => x).ToList();
        if (_onlyUnprocessed) result = result.Where(i => !GetOrCreateItemRow(i).IsProcessed).ToList();
        return result;
    }

    private ItemRow GetOrCreateItemRow(int itemId)
    {
        var existing = _allRows.FirstOrDefault(r => r.ItemId == itemId);
        if (existing is not null) return existing;
        var info = _itemsData[itemId];
        return new ItemRow(info, GetOrCreateItemProgress(itemId), GetOrCreateCategoryProgress(info.CategoryId));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Editor commands
    // ──────────────────────────────────────────────────────────────────────────

    private void ApplyItemSettings()
    {
        if (_selectedItem is null) return;
        if (!TryParseGroupChance(ItemGroupText, ItemChanceText, out int? grp, out double? chc)) return;

        var ip = GetOrCreateItemProgress(_selectedItem.ItemId);
        ip.Group = grp;
        ip.Chance = chc;
        _selectedItem.ItemProgress = ip;

        _ = TrySilentSave();
        ItemsView.Refresh();
        RefreshSummary();
    }

    private void ClearItemSettings()
    {
        if (_selectedItem is null) return;
        _progress.Items.Remove(_selectedItem.ItemId.ToString());
        _selectedItem.ItemProgress = new ItemProgress();
        ItemGroupText = "";
        ItemChanceText = "";
        _ = TrySilentSave();
        ItemsView.Refresh();
        RefreshSummary();
    }

    private void ApplyAllFromDb()
    {
        var candidates = _allRows
            .Where(r => r.DbGroup.HasValue && r.DbChance.HasValue && r.ItemGroup is null)
            .ToList();

        if (candidates.Count == 0)
        {
            MessageBox.Show("Нет предметов с данными из БД, у которых ещё не задана группа предмета.",
                "Применить из БД", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Применить Группу (БД) и Шанс (БД) как item-level значения для {candidates.Count} предмет(ов)?",
            "Применить из БД", MessageBoxButton.OKCancel, MessageBoxImage.Question);
        if (result != MessageBoxResult.OK) return;

        foreach (var row in candidates)
        {
            var ip = GetOrCreateItemProgress(row.ItemId);
            ip.Group = row.DbGroup;
            ip.Chance = row.DbChance;
            row.ItemProgress = ip;
        }

        _ = TrySilentSave();
        ItemsView.Refresh();
        RefreshSummary();
        StatusText = _ui.StatusAppliedFromDb(candidates.Count);
    }

    private static void ShowGroupHelp()
    {
        MessageBox.Show(
            """
            КАК РАБОТАЮТ ГРУППЫ (group) В ТАБЛИЦЕ loots
            ═══════════════════════════════════════════

            ГРУППА 0 — индивидуальный дроп
            ──────────────────────────────
            Каждый предмет из группы 0 проверяется
            НЕЗАВИСИМО. Шанс выпадения = drop_rate
            предмета. Могут выпасть несколько или
            все предметы одновременно.

            Пример: 3 предмета с 50% → каждый
            проверяется отдельно, могут выпасть все.

            ГРУППА 1, 2, 3... — "мешок лута"
            ────────────────────────────────────
            1. Сначала бросается кубик против
               loot_groups.drop_rate этой группы.
               Если провал → вся группа пропускается.

            2. Если группа сыграла → выбирается
               ОДИН предмет из группы (по весу
               drop_rate каждого предмета).

            Реальный пример из БД (pack_id=9327):
              • loot_groups: group_no=1, drop_rate=41666
                → шанс открыть "мешок" = 0.42%
              • loots: 952 предмета в group=1
                с разными drop_rate (веса выбора)
              → При убийстве NPC: сначала 0.42% шанс
                что мешок откроется, затем из 952
                предметов выпадает ровно ОДИН.

            НЕСКОЛЬКО ГРУПП у одного pack_id:
            ────────────────────────────────────
            Каждая группа — отдельный независимый
            "мешок". Все группы проверяются при
            каждом убийстве. Может выпасть по
            одному предмету из каждой группы.

            ═══════════════════════════════════════════
            ИТОГ:
              group=0  → каждый предмет независимо
              group=1+ → "мешок": сначала шанс открытия
                         (loot_groups.drop_rate), затем
                         1 предмет из группы по весам

            drop_rate / 10 000 000 = шанс в долях
            Пример: 500 000 / 10 000 000 = 5%
            """,
            "Справка: группы лута",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void ToggleLanguage()
    {
        _preferRussian = !_preferRussian;
        _ui = new UIStrings(_preferRussian);
        _colSvc.SavePreferRussian(_preferRussian);

        OnPropertyChanged(nameof(PreferRussian));
        OnPropertyChanged(nameof(LanguageLabel));
        OnPropertyChanged(nameof(UI));
        OnPropertyChanged(nameof(NpcListLabelText));
        OnPropertyChanged(nameof(NpcDetailLabelText));
        OnPropertyChanged(nameof(SelectedItemInfoText));
        RefreshSummary();

        // Update initial status text if no data loaded
        if (_allRows.Count == 0)
            StatusText = _ui.InitialStatus;

        // Notify code-behind to re-apply column headers
        LanguageChanged?.Invoke();

        if (!string.IsNullOrEmpty(_progress.DbPath) && _progress.SourceJsonPaths.Count > 0)
            _ = LoadDataAsync(_progress.DbPath, _progress.SourceJsonPaths);
    }

    /// <summary>Fired when language changes so code-behind can update DataGrid column headers.</summary>
    public event Action? LanguageChanged;

    private void ApplyCategorySettings()
    {
        if (_selectedItem is null) return;
        if (!TryParseGroupChance(CategoryGroupText, CategoryChanceText, out int? grp, out double? chc)) return;

        var cp = GetOrCreateCategoryProgress(_selectedItem.CategoryId);
        cp.Group = grp;
        cp.Chance = chc;

        // Update all rows that share this category
        foreach (var row in _allRows.Where(r => r.CategoryId == _selectedItem.CategoryId))
            row.CategoryProgress = cp;

        _ = TrySilentSave();
        ItemsView.Refresh();
        RefreshSummary();
    }

    private void ClearCategorySettings()
    {
        if (_selectedItem is null) return;
        _progress.Categories.Remove(_selectedItem.CategoryId.ToString());
        var emptycp = new CategoryProgress();
        foreach (var row in _allRows.Where(r => r.CategoryId == _selectedItem.CategoryId))
            row.CategoryProgress = emptycp;

        CategoryGroupText = "";
        CategoryChanceText = "";
        _ = TrySilentSave();
        ItemsView.Refresh();
        RefreshSummary();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Navigation
    // ──────────────────────────────────────────────────────────────────────────

    private void SelectPrev() => MoveSelection(-1);
    private void SelectNext() => MoveSelection(+1);

    private void MoveSelection(int delta)
    {
        var rows = ItemsView.Cast<ItemRow>().ToList();
        if (rows.Count == 0) return;
        int idx = _selectedItem is null ? 0 : rows.IndexOf(_selectedItem);
        int next = Math.Clamp(idx + delta, 0, rows.Count - 1);
        SelectedItem = rows[next];
        ScrollIntoViewRequested?.Invoke(rows[next]);
    }

    private void SelectNextUnprocessed()
    {
        var rows = ItemsView.Cast<ItemRow>().ToList();
        if (rows.Count == 0) return;
        int start = _selectedItem is null ? -1 : rows.IndexOf(_selectedItem);

        for (int i = start + 1; i < rows.Count; i++)
            if (!rows[i].IsProcessed) { SelectedItem = rows[i]; ScrollIntoViewRequested?.Invoke(rows[i]); return; }
        for (int i = 0; i <= start; i++)
            if (!rows[i].IsProcessed) { SelectedItem = rows[i]; ScrollIntoViewRequested?.Invoke(rows[i]); return; }

        MessageBox.Show("Необработанных предметов больше нет.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void JumpToSelectedNpcItem()
    {
        // Routed from view — NpcItemRow selection resolved via SelectedNpcDetailItem property
        if (SelectedNpcDetailItem is null) return;
        var target = _allRows.FirstOrDefault(r => r.ItemId == SelectedNpcDetailItem.ItemId);
        if (target is null) return;

        // Clear filters so the target row is visible
        SearchText = "";
        OnlyUnprocessed = false;

        SelectedItem = target;

        // Defer scroll until after ItemsView.Refresh() is processed by the UI
        Application.Current.Dispatcher.InvokeAsync(
            () => ScrollIntoViewRequested?.Invoke(target),
            System.Windows.Threading.DispatcherPriority.Background);
    }

    private NpcItemRow? _selectedNpcDetailItem;
    public NpcItemRow? SelectedNpcDetailItem
    {
        get => _selectedNpcDetailItem;
        set => Set(ref _selectedNpcDetailItem, value);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Save / Export
    // ──────────────────────────────────────────────────────────────────────────

    private async Task SaveProgressAsync()
    {
        try
        {
            await _progressSvc.SaveAsync(_progress);
            StatusText = _ui.StatusSaved;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task TrySilentSave()
    {
        try { await _progressSvc.SaveAsync(_progress); }
        catch { /* silent */ }
    }

    private async Task ExportAsync()
    {
        var dlg = new SaveFileDialog
        {
            Title = "Сохранить результат",
            DefaultExt = ".json",
            Filter = "JSON (*.json)|*.json|Все файлы|*.*"
        };
        if (dlg.ShowDialog() != true) return;

        var items = _allRows
            .Select(r => (r.ItemId, r.EffectiveChance))
            .ToList();

        try
        {
            await ProgressService.ExportAsync(dlg.FileName, items);
            StatusText = _ui.StatusExported(dlg.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка экспорта:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task SyncDbAsync()
    {
        // Collect only rows that have both group and chance assigned
        var syncItems = _allRows
            .Where(r => r.EffectiveGroup.HasValue && r.EffectiveChance.HasValue)
            .Select(r =>
            {
                var npcIds = _itemsData.TryGetValue(r.ItemId, out var info) ? info.NpcIds : (IReadOnlyList<int>)[];
                return new SyncItem(r.ItemId, npcIds, r.EffectiveGroup!.Value, r.EffectiveChance!.Value);
            })
            .ToList();

        if (syncItems.Count == 0)
        {
            MessageBox.Show("Нет предметов с назначенными группой и шансом.", "Нечего записывать",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Preview
        var svc = new DbSyncService(EffectiveLootDbPath);
        DbSyncService.SyncPreview preview;
        try
        {
            StatusText = _ui.StatusCounting;
            preview = await svc.PreviewAsync(syncItems);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при анализе БД:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText = _ui.StatusWriteError;
            return;
        }

        var confirm = MessageBox.Show(
            $"Будет обновлено:\n" +
            $"  loots:       обновить {preview.ToUpdate}, добавить {preview.ToInsert}\n" +
            $"  loot_groups: обновить {preview.LootGroupRows}\n\n" +
            $"Продолжить?",
            "Запись в БД", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) { StatusText = _ui.StatusWriteCancelled; return; }

        // Sync
        IsLoading = true;
        try
        {
            var progress = new Progress<string>(msg => StatusText = msg);
            await svc.SyncAsync(syncItems, progress);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка записи в БД:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText = _ui.StatusWriteError;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ImportSqlAsync()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Выбери SQL файл(ы) для импорта",
            Filter = "SQL files (*.sql)|*.sql|All files (*.*)|*.*",
            Multiselect = true,
        };
        if (dlg.ShowDialog() != true) return;

        var fileList = string.Join("\n", dlg.FileNames.Select(Path.GetFileName));
        var confirm = MessageBox.Show(
            $"Выполнить SQL-инструкции из {dlg.FileNames.Length} файл(а/ов):\n{fileList}\n\nЭто изменит базу данных. Продолжить?",
            "Импорт SQL", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        StatusText = _ui.StatusImporting;
        try
        {
            var svc = new DbSyncService(EffectiveLootDbPath);
            var progress = new Progress<string>(msg => StatusText = msg);

            int totalInserted = 0, totalUpdated = 0, totalOther = 0;
            foreach (var filePath in dlg.FileNames)
            {
                var result = await svc.ImportSqlFileAsync(filePath, progress);
                int updated = result.Replaced + result.Updated;
                totalInserted += result.Inserted;
                totalUpdated  += updated;
                totalOther    += result.Other;
                _sqlImports[Path.GetFileName(filePath)] = (result.Inserted, updated);
            }

            RefreshSummary();
            StatusText = _ui.StatusImportDone(totalInserted, totalUpdated);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Импорт завершён успешно ({dlg.FileNames.Length} файл(а/ов)).");
            sb.AppendLine();
            sb.AppendLine($"Добавлено:  {totalInserted}");
            sb.AppendLine($"Обновлено:  {totalUpdated}");
            if (totalOther > 0)
                sb.AppendLine($"Прочих:     {totalOther}");
            sb.AppendLine($"Итого:      {totalInserted + totalUpdated + totalOther}");
            MessageBox.Show(sb.ToString(), "Импорт SQL", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка импорта SQL:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText = _ui.StatusImportError;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Summary
    // ──────────────────────────────────────────────────────────────────────────

    private void RefreshSummary()
    {
        int total = _allRows.Count;
        int done = 0, itemDone = 0, catDone = 0, withChance = 0;

        foreach (var row in _allRows)
        {
            if (row.ItemProgress.Group.HasValue) { done++; itemDone++; }
            else if (row.CategoryProgress.Group.HasValue) { done++; catDone++; }
            if (row.EffectiveChance.HasValue) withChance++;
        }

        SummaryLeft = _ui.SummaryLeft(total, done, itemDone, catDone, withChance);

        int totalIns = _sqlImports.Values.Sum(x => x.Inserted);
        int totalUpd = _sqlImports.Values.Sum(x => x.Updated);
        SummaryRight = _ui.SummaryRight(totalIns, totalUpd);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private ItemProgress GetOrCreateItemProgress(int itemId)
    {
        var key = itemId.ToString();
        if (!_progress.Items.TryGetValue(key, out var ip))
        {
            ip = new ItemProgress();
            _progress.Items[key] = ip;
        }
        return ip;
    }

    private CategoryProgress GetOrCreateCategoryProgress(int catId)
    {
        var key = catId.ToString();
        if (!_progress.Categories.TryGetValue(key, out var cp))
        {
            cp = new CategoryProgress();
            _progress.Categories[key] = cp;
        }
        return cp;
    }

    private static bool TryParseGroupChance(
        string groupText, string chanceText,
        out int? group, out double? chance)
    {
        group = null;
        chance = null;

        groupText = groupText.Trim();
        chanceText = chanceText.Trim().Replace(',', '.');

        if (!string.IsNullOrEmpty(groupText))
        {
            if (!int.TryParse(groupText, out int g))
            {
                MessageBox.Show("Группа должна быть целым числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            group = g;
        }

        if (!string.IsNullOrEmpty(chanceText))
        {
            if (!double.TryParse(chanceText, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double c))
            {
                MessageBox.Show("Шанс должен быть числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            chance = c;
        }

        return true;
    }

    private void ClearEditor()
    {
        ItemGroupText = "";
        ItemChanceText = "";
        CategoryGroupText = "";
        CategoryChanceText = "";
    }

    // ──────────────────────────────────────────────────────────────────────────
    // INPC
    // ──────────────────────────────────────────────────────────────────────────

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// ──────────────────────────────────────────────────────────────────────────────
// Helper DTO for the NPC items sub-grid
// ──────────────────────────────────────────────────────────────────────────────

public sealed class NpcItemRow(
    int itemId,
    string itemName,
    string categoryName,
    int? lootPackId,
    int? effectiveGroup,
    double? effectiveChance,
    string highlightState)
{
    public int ItemId { get; } = itemId;
    public string ItemName { get; } = itemName;
    public string CategoryName { get; } = categoryName;
    public int? LootPackId { get; } = lootPackId;
    public int? EffectiveGroup { get; } = effectiveGroup;
    public double? EffectiveChance { get; } = effectiveChance;
    public string HighlightState { get; } = highlightState;
}
