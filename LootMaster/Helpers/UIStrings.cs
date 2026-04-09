namespace LootMaster.Helpers;

/// <summary>
/// All user-visible UI strings in two languages.
/// ViewModel exposes an instance as the <c>UI</c> property and replaces it when language changes.
/// XAML binds via <c>{Binding UI.XxxProperty}</c>.
/// </summary>
public sealed class UIStrings
{
    private readonly bool _ru;
    public UIStrings(bool ru) => _ru = ru;
    public bool IsRussian => _ru;

    private string R(string ru, string en) => _ru ? ru : en;

    // ── Toolbar ───────────────────────────────────────────────────────────────

    public string OpenDb        => R("Открыть SQLite базу",  "Open SQLite DB");
    public string OpenDbTip     => R("База с предметами, NPC, категориями (compact.sqlite3)",
                                     "DB with items, NPCs, categories (compact.sqlite3)");
    public string OpenLootDb    => R("Открыть БД лута",      "Open Loot DB");
    public string OpenLootDbTip => R("База с таблицами лута: loots, loot_groups, loot_pack_dropping_npcs (compact.server.table.sqlite3)",
                                     "DB with loot tables: loots, loot_groups, loot_pack_dropping_npcs (compact.server.table.sqlite3)");
    public string OpenJson      => R("Открыть JSON дропа",   "Open Drop JSON");
    public string SaveProgress  => R("Сохранить прогресс",   "Save Progress");
    public string Export        => R("Экспорт результата",   "Export Result");
    public string WriteToDb     => R("Записать в БД",        "Write to DB");
    public string ImportSql     => R("Импорт SQL в БД",      "Import SQL to DB");
    public string Search        => R("Поиск:",               "Search:");
    public string OnlyUnproc    => R("Только необработанные","Unprocessed only");
    public string HlCategories  => R("Подсвечивать категории","Highlight categories");
    public string ShowNpcId     => R("Кол. NPC ID",          "Show NPC ID");
    public string ShowNpcNames  => R("Кол. NPC имена",       "Show NPC names");
    public string LangTip       => R("Переключить язык названий предметов и NPC (RU / EN)",
                                     "Toggle language for item and NPC names (RU / EN)");

    // ── DataGrid column headers ───────────────────────────────────────────────

    public string ColItem       => R("Предмет",             "Item");
    public string ColCategory   => R("Категория",           "Category");
    public string ColGroupItem  => R("Группа (предмет)",    "Group (item)");
    public string ColChanceItem => R("Шанс (предмет)",      "Chance (item)");
    public string ColGroupCat   => R("Группа (категория)",  "Group (cat)");
    public string ColChanceCat  => R("Шанс (категория)",    "Chance (cat)");
    public string ColGroupEff   => R("Итог. группа",        "Eff. group");
    public string ColChanceEff  => R("Итог. шанс",         "Eff. chance");
    public string ColGroupDb    => R("Группа (БД)",         "Group (DB)");
    public string ColChanceDb   => R("Шанс (БД)",           "Chance (DB)");
    public string ColGroup      => R("Группа",              "Group");
    public string ColChance     => R("Шанс",                "Chance");

    // ── Right panel GroupBox headers ──────────────────────────────────────────

    public string GbSelectedItem    => R("Выбранный предмет",       "Selected Item");
    public string GbItemSettings    => R("Настройки предмета",      "Item Settings");
    public string GbCatSettings     => R("Настройки категории",     "Category Settings");
    public string GbNavigation      => R("Навигация",               "Navigation");
    public string GbNpcBrowser      => R("NPC выбранного предмета", "NPCs of Selected Item");
    public string GbSummary         => R("Сводка",                  "Summary");

    // ── Item settings ─────────────────────────────────────────────────────────

    public string ItemGroupLabel    => R("Группа предмета:",        "Item group:");
    public string ItemChanceLabel   => R("Шанс предмета:",          "Item chance:");
    public string BtnApply          => R("Применить",               "Apply");
    public string BtnReset          => R("Сбросить",                "Reset");
    public string BtnApplyAllFromDb => R("Применить из БД ко всем", "Apply DB to all");
    public string BtnApplyAllTip    => R("Применить Группу (БД) и Шанс (БД) как item-level значения для всех предметов, у которых ещё не задана группа",
                                         "Apply Group (DB) and Chance (DB) as item-level values for all items without a manually set group");
    public string GroupHelpTip      => R("Справка по группам лута", "Loot group help");

    // ── Category settings ─────────────────────────────────────────────────────

    public string CatGroupLabel     => R("Группа категории:",            "Category group:");
    public string CatChanceLabel    => R("Шанс категории:",              "Category chance:");
    public string BtnApplyCat       => R("Применить ко всей категории",  "Apply to whole category");

    // ── Navigation ────────────────────────────────────────────────────────────

    public string BtnPrev           => R("← Предыдущий",        "← Previous");
    public string BtnNext           => R("Следующий →",          "Next →");
    public string BtnNextUnproc     => R("Следующий необработ.", "Next unprocessed");

    // ── NPC browser ───────────────────────────────────────────────────────────

    public string NpcLabel          => "NPC:";
    public string InfoFormat        => R("Item ID: {0}\nПредмет: {1}\nCat ID: {2}\nКатегория: {3}",
                                         "Item ID: {0}\nItem: {1}\nCat ID: {2}\nCategory: {3}");

    public string NpcListLabel(int count)   => R($"NPC, с которых падает предмет: ({count})",
                                                  $"NPCs dropping item: ({count})");
    public string NpcDetailLabel(int count) => R($"Все предметы выбранного NPC: ({count})",
                                                  $"All items of selected NPC: ({count})");

    // ── Summary ───────────────────────────────────────────────────────────────

    public string SummaryLeft(int total, int done, int itemDone, int catDone, int withChance) =>
        _ru
            ? $"Всего предметов:   {total}\n" +
              $"Обработано:        {done}\n"  +
              $"  напрямую:        {itemDone}\n" +
              $"  через категорию: {catDone}\n" +
              $"С итоговым шансом: {withChance}"
            : $"Total items:       {total}\n" +
              $"Processed:         {done}\n"  +
              $"  directly:        {itemDone}\n" +
              $"  via category:    {catDone}\n" +
              $"With final chance: {withChance}";

    public string SummaryRight(int ins, int upd) =>
        _ru
            ? $"── Импорт SQL ──\nДобавлено: {ins}\nОбновлено: {upd}\nИтого:     {ins + upd}"
            : $"── SQL Import ──\nInserted:  {ins}\nUpdated:   {upd}\nTotal:     {ins + upd}";

    // ── Status messages ───────────────────────────────────────────────────────

    public string InitialStatus     => R("Загрузи SQLite базу и JSON дропа",
                                         "Open SQLite DB and drop JSON files");
    public string StatusParsingJson => R("Парсим JSON…",         "Parsing JSON…");
    public string StatusLoadingDb   => R("Загрузи SQLite базу и JSON дропа", "Loading from DB…");
    public string StatusReady(int items, int npcs, int cats) =>
        _ru ? $"Готово. Предметов: {items}, NPC: {npcs}, категорий: {cats}"
            : $"Ready. Items: {items}, NPCs: {npcs}, categories: {cats}";
    public string StatusCancelled   => R("Загрузка отменена.",   "Load cancelled.");
    public string StatusLoadError   => R("Ошибка загрузки.",     "Load error.");
    public string StatusSaved       => R("Прогресс сохранён.",   "Progress saved.");
    public string StatusCounting    => R("Подсчёт затронутых строк…", "Counting affected rows…");
    public string StatusWriteCancelled => R("Запись отменена.",  "Write cancelled.");
    public string StatusWriteError  => R("Ошибка записи в БД.",  "DB write error.");
    public string StatusImporting   => R("Импорт SQL…",          "Importing SQL…");
    public string StatusImportError => R("Ошибка импорта SQL.",  "SQL import error.");
    public string StatusAppliedFromDb(int count) =>
        _ru ? $"Применено значений из БД: {count}"
            : $"Applied DB values: {count}";
    public string StatusDbOpened(string name)   => _ru ? $"База данных: {name}"   : $"Database: {name}";
    public string StatusLootDbOpened(string name) => _ru ? $"База лута: {name}"   : $"Loot DB: {name}";
    public string StatusExported(string path)   => _ru ? $"Экспорт сохранён: {path}" : $"Exported: {path}";
    public string StatusImportDone(int ins, int upd) =>
        _ru ? $"Импорт завершён. Добавлено: {ins}, обновлено: {upd}"
            : $"Import done. Inserted: {ins}, updated: {upd}";
}
