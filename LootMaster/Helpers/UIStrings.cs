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
    public string ImportFromDb  => R("Импорт из БД",         "Import from DB");
    public string ImportFromDbTip => R("Скопировать таблицы лута из другой SQLite базы в текущую БД лута (upsert по id)",
                                       "Copy loot tables from another SQLite database into the current loot DB (upsert by id)");
    public string Search        => R("Поиск:",               "Search:");
    public string OnlyUnproc    => R("Только необработанные","Unprocessed only");
    public string HlCategories  => R("Подсвечивать категории","Highlight categories");
    public string ShowNpcId     => R("Кол. NPC ID",          "Show NPC ID");
    public string ShowNpcNames  => R("Кол. NPC имена",       "Show NPC names");
    public string LangTip       => R("Переключить язык названий предметов и NPC (RU / EN)",
                                     "Toggle language for item and NPC names (RU / EN)");

    // ── Toolbar tooltips ──────────────────────────────────────────────────────

    public string OpenJsonTip      => R("Выбери один или несколько JSON-файлов дропа. Поддерживаются форматы NPC-лута (npc_id) и Doodad-лута (doodad_id + loot_pack_id)",
                                        "Select one or more drop JSON files. Supports NPC loot (npc_id) and Doodad loot (doodad_id + loot_pack_id) formats");
    public string SaveProgressTip  => R("Сохранить текущий прогресс в файл (Ctrl+S). Прогресс также сохраняется автоматически после каждого изменения",
                                        "Save current progress to file (Ctrl+S). Progress is also auto-saved after every change");
    public string ExportTip        => R("Экспортировать итоговые данные в JSON-файл для использования в эмуляторе сервера",
                                        "Export final data to a JSON file for use in the server emulator");
    public string WriteTip         => R("Записать все назначенные группы и шансы напрямую в SQLite-базу лута (upsert таблиц loots и loot_groups). Перед записью будет показан предварительный просмотр",
                                        "Write all assigned groups and chances directly to the SQLite loot DB (upsert of loots and loot_groups). A preview will be shown before writing");
    public string ImportSqlTip     => R("Выполнить SQL-файл(ы) в базу данных лута. Поддерживаются обычный SQL и дампы Navicat. Существующие строки обновляются, новые добавляются",
                                        "Execute SQL file(s) into the loot database. Supports standard SQL and Navicat dumps. Existing rows are updated, new ones inserted");
    public string OnlyUnprocTip    => R("Скрыть предметы, которым уже назначена группа (зелёные и жёлтые строки)",
                                        "Hide items that already have a group assigned (green and yellow rows)");
    public string HlCatTip         => R("Подсвечивать жёлтым строки, у которых группа унаследована от категории (не задана напрямую для предмета)",
                                        "Highlight in yellow rows whose group is inherited from the category (not set directly on the item)");
    public string ShowNpcIdTip     => R("Показывать колонку с ID всех NPC, у которых падает предмет",
                                        "Show column with IDs of all NPCs that drop the item");
    public string ShowNpcNamesTip  => R("Показывать колонку с именами всех NPC, у которых падает предмет",
                                        "Show column with names of all NPCs that drop the item");

    // ── Right panel tooltips ──────────────────────────────────────────────────

    public string BtnApplyTip      => R("Сохранить введённые группу и шанс для выбранного предмета. Строка станет зелёной",
                                        "Save the entered group and chance for the selected item. The row will turn green");
    public string BtnResetItemTip  => R("Сбросить индивидуальную настройку предмета. Если для категории задана группа — предмет унаследует её (жёлтый), иначе станет белым",
                                        "Reset the item-level setting. If the category has a group set, the item inherits it (yellow), otherwise it becomes white");
    public string BtnApplyCatTip   => R("Применить группу и шанс ко всем предметам этой категории. Предметы с индивидуальной настройкой не затрагиваются",
                                        "Apply group and chance to all items in this category. Items with individual settings are not affected");
    public string BtnResetCatTip   => R("Сбросить настройку категории. Предметы, у которых нет индивидуальной настройки, станут белыми",
                                        "Reset the category setting. Items without an individual setting will turn white");
    public string BtnPrevTip       => R("Перейти к предыдущему предмету в отфильтрованном списке",
                                        "Go to the previous item in the filtered list");
    public string BtnNextTip       => R("Перейти к следующему предмету в отфильтрованном списке",
                                        "Go to the next item in the filtered list");
    public string BtnNextUnprocTip => R("Перейти к ближайшему предмету, которому ещё не назначена группа",
                                        "Go to the nearest item that does not yet have a group assigned");

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
    public string StatusImporting      => R("Импорт SQL…",          "Importing SQL…");
    public string StatusImportError    => R("Ошибка импорта SQL.",  "SQL import error.");
    public string StatusImportingFromDb => R("Импорт из БД…",        "Importing from DB…");
    public string StatusImportDbError  => R("Ошибка импорта из БД.", "DB import error.");
    public string StatusAppliedFromDb(int count) =>
        _ru ? $"Применено значений из БД: {count}"
            : $"Applied DB values: {count}";
    public string StatusDbOpened(string name)   => _ru ? $"База данных: {name}"   : $"Database: {name}";
    public string StatusLootDbOpened(string name) => _ru ? $"База лута: {name}"   : $"Loot DB: {name}";
    public string StatusExported(string path)   => _ru ? $"Экспорт сохранён: {path}" : $"Exported: {path}";
    public string StatusImportDone(int ins, int upd) =>
        _ru ? $"Импорт завершён. Добавлено: {ins}, обновлено: {upd}"
            : $"Import done. Inserted: {ins}, updated: {upd}";
    public string StatusImportDbDone(int ins, int upd) =>
        _ru ? $"Импорт из БД завершён. Добавлено: {ins}, обновлено: {upd}"
            : $"DB import done. Inserted: {ins}, updated: {upd}";
}
