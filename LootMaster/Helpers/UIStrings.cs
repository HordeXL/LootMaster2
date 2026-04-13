namespace LootMaster.Helpers;

/// <summary>
/// All user-visible UI strings in three languages: Russian, English, Simplified Chinese.
/// ViewModel exposes an instance as the <c>UI</c> property and replaces it when language changes.
/// XAML binds via <c>{Binding UI.XxxProperty}</c>.
/// Language codes: 0 = RU, 1 = EN, 2 = ZH
/// </summary>
public sealed class UIStrings
{
    private readonly int _lang; // 0=RU, 1=EN, 2=ZH
    public UIStrings(int lang) => _lang = lang;
    public int LanguageCode => _lang;
    public bool IsRussian => _lang == 0;

    private string T(string ru, string en, string zh) => _lang == 0 ? ru : (_lang == 1 ? en : zh);

    // ── Toolbar ───────────────────────────────────────────────────────────────

    public string OpenDb        => T("Открыть БД",  "Open DB", "打开数据库");
    public string OpenDbTip     => T("База с предметами, NPC, категориями (compact.sqlite3)",
                                     "DB with items, NPCs, categories (compact.sqlite3)",
                                     "包含物品、NPC、分类的数据库 (compact.sqlite3)");
    public string OpenLootDb    => T("Открыть БД лута",      "Open Loot DB", "打开战利品数据库");
    public string OpenLootDbTip => T("База с таблицами лута: loots, loot_groups, loot_pack_dropping_npcs (compact.server.table.sqlite3)",
                                     "DB with loot tables: loots, loot_groups, loot_pack_dropping_npcs (compact.server.table.sqlite3)",
                                     "包含战利品表的数据库: loots, loot_groups, loot_pack_dropping_npcs (compact.server.table.sqlite3)");
    public string OpenJson      => T("Открыть JSON дропа",   "Open Drop JSON", "打开掉落JSON");
    public string SaveProgress  => T("Сохранить прогресс",   "Save Progress", "保存进度");
    public string Export        => T("Экспорт результата",   "Export Result", "导出结果");
    public string WriteToDb     => T("Записать в БД",        "Write to DB", "写入数据库");
    public string ImportSql     => T("Импорт SQL в БД",      "Import SQL to DB", "导入SQL到数据库");
    public string ImportFromDb  => T("Генерация патча из БД",  "Generate Patch from DB", "从数据库生成补丁");
    public string ImportFromDbTip => T("Сравнить другую SQLite базу с текущей БД лута и создать SQL-патч-файл (INSERT/UPDATE для loots и loot_groups)",
                                       "Compare another SQLite database with the current loot DB and generate an SQL patch file (INSERT/UPDATE for loots and loot_groups)",
                                       "将另一个SQLite数据库与当前战利品数据库比较，生成SQL补丁文件 (对loots和loot_groups进行INSERT/UPDATE)");
    public string ExtractNpcDrops => T("Извлечь NPC дроп", "Extract NPC Drops", "提取NPC掉落");
    public string ExtractNpcDropsTip => T(
        "Извлечь данные о дропе NPC из базы данных и сохранить в JSON файл...",
        "Extract NPC drop data from database and save to JSON file...",
        "从数据库中提取NPC掉落数据并保存为JSON文件格式...");
    public string Search        => T("Поиск:",               "Search:", "搜索:");
    public string OnlyUnproc    => T("Только необработанные","Unprocessed only", "仅未处理");
    public string HlCategories  => T("Подсвечивать категории","Highlight categories", "高亮分类");
    public string ShowNpcId     => T("Кол. NPC ID",          "Show NPC ID", "显示NPC ID");
    public string ShowNpcNames  => T("Кол. NPC имена",       "Show NPC names", "显示NPC名称");
    public string LangTip       => T("Переключить язык названий предметов и NPC (RU / EN / ZH)",
                                     "Toggle language for item and NPC names (RU / EN / ZH)",
                                     "切换物品和NPC名称的语言，以及数据库内容的语言 (RU / EN / ZH)");

    // ── Toolbar tooltips ──────────────────────────────────────────────────────

    public string OpenJsonTip      => T("Выбери один или несколько JSON-файлов дропа. Поддерживаются форматы NPC-лута (npc_id) и Doodad-лута (doodad_id + loot_pack_id)",
                                        "Select one or more drop JSON files. Supports NPC loot (npc_id) and Doodad loot (doodad_id + loot_pack_id) formats",
                                        "选择一个或多个掉落JSON文件。支持NPC战利品 (npc_id) 和Doodad战利品 (doodad_id + loot_pack_id) 格式");
    public string SaveProgressTip  => T("Сохранить текущий прогресс в файл (Ctrl+S). Прогресс также сохраняется автоматически после каждого изменения",
                                        "Save current progress to file (Ctrl+S). Progress is also auto-saved after every change",
                                        "将当前进度保存到文件 (Ctrl+S)。每次更改后也会自动保存进度");
    public string ExportTip        => T("Экспортировать итоговые данные в JSON-файл для использования в эмуляторе сервера",
                                        "Export final data to a JSON file for use in the server emulator",
                                        "将最终数据导出为JSON文件，供服务器模拟器使用");
    public string WriteTip         => T("Записать все назначенные группы и шансы напрямую в SQLite-базу лута (upsert таблиц loots и loot_groups). Перед записью будет показан предварительный просмотр",
                                        "Write all assigned groups and chances directly to the SQLite loot DB (upsert of loots and loot_groups). A preview will be shown before writing",
                                        "将所有分配的组和几率直接写入SQLite战利品数据库 (更新或插入loots和loot_groups表)。写入前将显示预览");
    public string ImportSqlTip     => T("Выполнить SQL-файл(ы) в базу данных лута. Поддерживаются обычный SQL и дампы Navicat. Существующие строки обновляются, новые добавляются",
                                        "Execute SQL file(s) into the loot database. Supports standard SQL and Navicat dumps. Existing rows are updated, new ones inserted",
                                        "将SQL文件执行到战利品数据库。支持标准SQL和Navicat导出。现有行会被更新，新行会被插入");
    public string OnlyUnprocTip    => T("Скрыть предметы, которым уже назначена группа (зелёные и жёлтые строки)",
                                        "Hide items that already have a group assigned (green and yellow rows)",
                                        "隐藏已分配组的物品 (绿色和黄色行)");
    public string HlCatTip         => T("Подсвечивать жёлтым строки, у которых группа унаследована от категории (не задана напрямую для предмета)",
                                        "Highlight in yellow rows whose group is inherited from the category (not set directly on the item)",
                                        "高亮显示组继承自分类的行 (未直接为物品设置)");
    public string ShowNpcIdTip     => T("Показывать колонку с ID всех NPC, у которых падает предмет",
                                        "Show column with IDs of all NPCs that drop the item",
                                        "显示掉落该物品的所有NPC ID列");
    public string ShowNpcNamesTip  => T("Показывать колонку с именами всех NPC, у которых падает предмет",
                                        "Show column with names of all NPCs that drop the item",
                                        "显示掉落该物品的所有NPC名称列");

    // ── Right panel tooltips ──────────────────────────────────────────────────

    public string BtnApplyTip      => T("Сохранить введённые группу и шанс для выбранного предмета. Строка станет зелёной",
                                        "Save the entered group and chance for the selected item. The row will turn green",
                                        "保存所选物品的组和几率。该行将变为绿色");
    public string BtnResetItemTip  => T("Сбросить индивидуальную настройку предмета. Если для категории задана группа — предмет унаследует её (жёлтый), иначе станет белым",
                                        "Reset the item-level setting. If the category has a group set, the item inherits it (yellow), otherwise it becomes white",
                                        "重置物品级设置。如果分类已设置组，物品将继承它 (黄色)，否则变为白色");
    public string BtnApplyCatTip   => T("Применить группу и шанс ко всем предметам этой категории. Предметы с индивидуальной настройкой не затрагиваются",
                                        "Apply group and chance to all items in this category. Items with individual settings are not affected",
                                        "将组和几率应用到该分类的所有物品。具有个别设置的物品不受影响");
    public string BtnResetCatTip   => T("Сбросить настройку категории. Предметы, у которых нет индивидуальной настройки, станут белыми",
                                        "Reset the category setting. Items without an individual setting will turn white",
                                        "重置分类设置。没有个别设置的物品将变为白色");
    public string BtnPrevTip       => T("Перейти к предыдущему предмету в отфильтрованном списке",
                                        "Go to the previous item in the filtered list",
                                        "跳转到过滤列表中的上一件物品");
    public string BtnNextTip       => T("Перейти к следующему предмету в отфильтрованном списке",
                                        "Go to the next item in the filtered list",
                                        "跳转到过滤列表中的下一件物品");
    public string BtnNextUnprocTip => T("Перейти к ближайшему предмету, которому ещё не назначена группа",
                                        "Go to the nearest item that does not yet have a group assigned",
                                        "跳转到最近的未分配组物品");

    // ── DataGrid column headers ───────────────────────────────────────────────

    public string ColItem       => T("Предмет",             "Item", "物品");
    public string ColCategory   => T("Категория",           "Category", "分类");
    public string ColGroupItem  => T("Группа (предмет)",    "Group (item)", "组 (物品)");
    public string ColChanceItem => T("Шанс (предмет)",      "Chance (item)", "几率 (物品)");
    public string ColGroupCat   => T("Группа (категория)",  "Group (cat)", "组 (分类)");
    public string ColChanceCat  => T("Шанс (категория)",    "Chance (cat)", "几率 (分类)");
    public string ColGroupEff   => T("Итог. группа",        "Eff. group", "最终组");
    public string ColChanceEff  => T("Итог. шанс",         "Eff. chance", "最终几率");
    public string ColGroupDb    => T("Группа (БД)",         "Group (DB)", "组 (数据库)");
    public string ColChanceDb   => T("Шанс (БД)",           "Chance (DB)", "几率 (数据库)");
    public string ColGroup      => T("Группа",              "Group", "组");
    public string ColChance     => T("Шанс",                "Chance", "几率");
    public string ColMinAmount  => T("Мин. кол.",           "Min Qty", "最小数量");
    public string ColMaxAmount  => T("Макс. кол.",          "Max Qty", "最大数量");
    public string ColGradeId    => T("Грейд ID",            "Grade ID", "等级ID");
    public string ColAlwaysDrop => T("Всегда",              "Always", "总是掉落");

    // ── Right panel GroupBox headers ──────────────────────────────────────────

    public string GbSelectedItem    => T("Выбранный предмет",       "Selected Item", "所选物品");
    public string GbItemSettings    => T("Настройки предмета",      "Item Settings", "物品设置");
    public string GbCatSettings     => T("Настройки категории",     "Category Settings", "分类设置");
    public string GbNavigation      => T("Навигация",               "Navigation", "导航");
    public string GbNpcBrowser      => T("NPC выбранного предмета", "NPCs of Selected Item", "所选物品的NPC");
    public string GbSummary         => T("Сводка",                  "Summary", "摘要");

    // ── Item settings ─────────────────────────────────────────────────────────

    public string ItemGroupLabel    => T("Группа предмета:",        "Item group:", "物品组:");
    public string ItemChanceLabel   => T("Шанс предмета:",          "Item chance:", "物品几率:");
    public string ItemMinAmtLabel   => T("Мин. количество:",        "Min amount:", "最小数量:");
    public string ItemMaxAmtLabel   => T("Макс. количество:",       "Max amount:", "最大数量:");
    public string ItemGradeLabel    => T("Грейд ID:",               "Grade ID:", "等级ID:");
    public string ItemAlwaysLabel   => T("Всегда выпадает:",        "Always drop:", "总是掉落:");
    public string BtnApply          => T("Применить",               "Apply", "应用");
    public string BtnReset          => T("Сбросить",                "Reset", "重置");
    public string BtnApplyAllFromDb  => T("Применить из БД ко всем", "Apply DB to all", "应用数据库值到所有");
    public string BtnApplyAllTip     => T("Применить Группу (БД) и Шанс (БД) как item-level значения для всех предметов, у которых ещё не задана группа",
                                          "Apply Group (DB) and Chance (DB) as item-level values for all items without a manually set group",
                                          "将组 (数据库) 和几率 (数据库) 作为物品级值应用于所有未手动设置组的物品");
    public string BtnClearAllFromDb  => T("Отменить из БД",          "Undo DB apply", "撤销数据库应用");
    public string BtnClearAllTip     => T("Сбросить item-level группу и шанс у всех предметов, у которых группа совпадает с Группой (БД) — отменяет действие «Применить из БД ко всем»",
                                          "Reset item-level group and chance for all items whose group matches Group (DB) — undoes 'Apply DB to all'",
                                          "重置所有组与组 (数据库) 匹配的物品的物品级组和几率 — 撤销'应用数据库值到所有'");
    public string GroupHelpTip      => T("Справка по группам лута", "Loot group help", "战利品组帮助");
    public string GroupHelpTitle    => T("Справка: группы лута",    "Help: loot groups", "帮助: 战利品组");
    public string GroupHelpText     => _lang == 0 ? """
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
            """ : _lang == 1 ? """
            HOW GROUPS WORK IN THE loots TABLE
            ═══════════════════════════════════════════

            GROUP 0 — individual drops
            ──────────────────────────────
            Each item in group 0 is checked
            INDEPENDENTLY. Drop chance = the item's
            drop_rate. Multiple or all items can drop
            at the same time.

            Example: 3 items at 50% → each is checked
            separately; all three can drop at once.

            GROUP 1, 2, 3... — "loot bag"
            ────────────────────────────────────
            1. First, a roll is made against
               loot_groups.drop_rate for the group.
               If it fails → the entire group is skipped.

            2. If the group fires → exactly ONE item
               is chosen from the group (weighted by
               each item's drop_rate).

            Real example from DB (pack_id=9327):
              • loot_groups: group_no=1, drop_rate=41666
                → 0.42% chance to open the "bag"
              • loots: 952 items in group=1
                with varying drop_rate (selection weights)
              → On NPC kill: first a 0.42% chance the
                bag opens, then exactly ONE item is
                chosen from the 952.

            MULTIPLE GROUPS for one pack_id:
            ────────────────────────────────────
            Each group is a separate, independent
            "bag". All groups are rolled on every kill.
            One item per group can drop per kill.

            ═══════════════════════════════════════════
            SUMMARY:
              group=0  → each item checked independently
              group=1+ → "bag": first an open chance
                         (loot_groups.drop_rate), then
                         1 item from the group by weight

            drop_rate / 10 000 000 = chance as fraction
            Example: 500 000 / 10 000 000 = 5%
            """ : """
            loots 表中的组是如何工作的
            ═══════════════════════════════════════════

            组 0 — 独立掉落
            ──────────────────────────────
            组 0 中的每个物品都会独立检查。
            掉落几率 = 物品的 drop_rate。
            多个或所有物品都可能同时掉落。

            示例: 3个物品各50% → 每个独立检查，
            三个可能同时掉落。

            组 1, 2, 3... — "战利品袋"
            ────────────────────────────────────
            1. 首先针对该组掷骰子，对照
               loot_groups.drop_rate。
               如果失败 → 整个组被跳过。

            2. 如果组触发 → 从组中选择恰好一件物品
               (按每件物品的 drop_rate 权重选择)。

            数据库实例 (pack_id=9327):
              • loot_groups: group_no=1, drop_rate=41666
                → "袋子" 开启几率 = 0.42%
              • loots: 952件物品在 group=1 中
                各有不同的 drop_rate (选择权重)
              → 击杀NPC时: 首先0.42%几率开启袋子，
                然后从952件物品中选择恰好一件。

            一个 pack_id 有多个组:
            ────────────────────────────────────
            每个组都是一个独立的"战利品袋"。
            每次击杀都会对所有组进行掷骰。
            每个组每次击杀可以掉落一件物品。

            ═══════════════════════════════════════════
            总结:
              group=0  → 每个物品独立检查
              group=1+ → "战利品袋": 先有开启几率
                         (loot_groups.drop_rate)，然后
                         按权重从组中选择1件物品

            drop_rate / 10 000 000 = 几率分数
            示例: 500 000 / 10 000 000 = 5%
            """;

    // ── Category settings ─────────────────────────────────────────────────────

    public string CatGroupLabel     => T("Группа категории:",            "Category group:", "分类组:");
    public string CatChanceLabel    => T("Шанс категории:",              "Category chance:", "分类几率:");
    public string CatGradeLabel     => T("Грейд ID:",                    "Grade ID:", "等级ID:");
    public string CatAlwaysLabel    => T("Всегда выпадает:",             "Always drop:", "总是掉落:");
    public string BtnApplyCat       => T("Применить ко всей категории",  "Apply to whole category", "应用到整个分类");

    // ── Navigation ────────────────────────────────────────────────────────────

    public string BtnPrev           => T("← Предыдущий",        "← Previous", "← 上一个");
    public string BtnNext           => T("Следующий →",          "Next →", "下一个 →");
    public string BtnNextUnproc     => T("Следующий необработ.", "Next unprocessed", "下一个未处理");

    // ── NPC browser ───────────────────────────────────────────────────────────

    public string NpcLabel          => "NPC:";
    public string InfoFormat        => T("Item ID: {0}\nПредмет: {1}\nCat ID: {2}\nКатегория: {3}",
                                         "Item ID: {0}\nItem: {1}\nCat ID: {2}\nCategory: {3}",
                                         "物品ID: {0}\n物品: {1}\n分类ID: {2}\n分类: {3}");

    public string NpcListLabel(int count)   => T($"NPC, с которых падает предмет: ({count})",
                                                  $"NPCs dropping item: ({count})",
                                                  $"掉落该物品的NPC: ({count})");
    public string NpcDetailLabel(int count) => T($"Все предметы выбранного NPC: ({count})",
                                                  $"All items of selected NPC: ({count})",
                                                  $"所选NPC的所有物品: ({count})");

    // ── Summary ───────────────────────────────────────────────────────────────

    public string SummaryLeft(int total, int done, int itemDone, int catDone, int withChance) =>
        _lang == 0
            ? $"Всего предметов:   {total}\n" +
              $"Обработано:        {done}\n"  +
              $"  напрямую:        {itemDone}\n" +
              $"  через категорию: {catDone}\n" +
              $"С итоговым шансом: {withChance}"
            : _lang == 1
            ? $"Total items:       {total}\n" +
              $"Processed:         {done}\n"  +
              $"  directly:        {itemDone}\n" +
              $"  via category:    {catDone}\n" +
              $"With final chance: {withChance}"
            : $"物品总数:          {total}\n" +
              $"已处理:            {done}\n"  +
              $"  直接设置:        {itemDone}\n" +
              $"  通过分类:        {catDone}\n" +
              $"有最终几率:        {withChance}";

    public string SummaryRightSql(int ins, int upd) =>
        _lang == 0
            ? $"── Импорт SQL ──\nДобавлено: {ins}\nОбновлено: {upd}\nИтого:     {ins + upd}"
            : _lang == 1
            ? $"── SQL Import ──\nInserted:  {ins}\nUpdated:   {upd}\nTotal:     {ins + upd}"
            : $"── SQL 导入 ──\n已插入:  {ins}\n已更新:  {upd}\n总计:    {ins + upd}";

    public string SummaryRightDb(int ins, int upd) =>
        _lang == 0
            ? $"── Генерация патча ──\nДобавлено: {ins}\nОбновлено: {upd}\nИтого:     {ins + upd}"
            : _lang == 1
            ? $"── Patch Gen ──\nInserted:  {ins}\nUpdated:   {upd}\nTotal:     {ins + upd}"
            : $"── 补丁生成 ──\n已插入:  {ins}\n已更新:  {upd}\n总计:      {ins + upd}";

    // ── Status messages ───────────────────────────────────────────────────────

    public string InitialStatus     => T("Загрузи SQLite базу и JSON дропа",
                                         "Open SQLite DB and drop JSON files",
                                         "打开SQLite数据库和掉落JSON文件");
    public string StatusParsingJson => T("Парсим JSON…",         "Parsing JSON…", "解析JSON…");
    public string StatusLoadingDb   => T("Загрузи SQLite базу и JSON дропа", "Loading from DB…", "从数据库加载…");
    public string StatusReady(int items, int npcs, int cats) =>
        _lang == 0 ? $"Готово. Предметов: {items}, NPC: {npcs}, категорий: {cats}"
            : _lang == 1 ? $"Ready. Items: {items}, NPCs: {npcs}, categories: {cats}"
            : $"完成. 物品: {items}, NPC: {npcs}, 分类: {cats}";
    public string StatusCancelled   => T("Загрузка отменена.",   "Load cancelled.", "加载已取消。");
    public string StatusLoadError   => T("Ошибка загрузки.",     "Load error.", "加载错误。");
    public string StatusSaved       => T("Прогресс сохранён.",   "Progress saved.", "进度已保存。");
    public string StatusCounting    => T("Подсчёт затронутых строк…", "Counting affected rows…", "计算受影响的行数…");
    public string StatusWriteCancelled => T("Запись отменена.",  "Write cancelled.", "写入已取消。");
    public string StatusWriteError  => T("Ошибка записи в БД.",  "DB write error.", "数据库写入错误。");
    public string StatusImporting      => T("Импорт SQL…",          "Importing SQL…", "导入SQL…");
    public string StatusImportError    => T("Ошибка импорта SQL.",  "SQL import error.", "SQL导入错误。");
    public string StatusImportingFromDb => T("Генерация патча…",     "Generating patch…", "生成补丁…");
    public string StatusImportDbError  => T("Ошибка генерации патча.", "Patch generation error.", "补丁生成错误。");
    public string PatchReadyTitle     => T("Генерация патча — готово", "Patch generation ready", "补丁生成完成");
    public string PatchApplyHint      => T("Файл сохранён в:\n{0}\n\nПрименить можно через «Импорт SQL в БД».",
                                            "File saved to:\n{0}\n\nApply it via 'Import SQL to DB'.",
                                            "文件保存到:\n{0}\n\n可通过 '导入SQL到数据库' 应用。");
    public string StatusAppliedFromDb(int count) =>
        _lang == 0 ? $"Применено значений из БД: {count}"
            : _lang == 1 ? $"Applied DB values: {count}"
            : $"已应用的数据库值: {count}";
    public string StatusDbOpened(string name)   => _lang == 0 ? $"База данных: {name}"   : _lang == 1 ? $"Database: {name}"   : $"数据库: {name}";
    public string StatusLootDbOpened(string name) => _lang == 0 ? $"База лута: {name}"   : _lang == 1 ? $"Loot DB: {name}"   : $"战利品数据库: {name}";
    public string StatusExported(string path)   => _lang == 0 ? $"Экспорт сохранён: {path}" : _lang == 1 ? $"Exported: {path}" : $"已导出: {path}";
    public string StatusPatchSaved(string name) => _lang == 0 ? $"Патч сохранён: {name}" : _lang == 1 ? $"Patch saved: {name}" : $"补丁已保存: {name}";
    public string PatchGenerated(string name)   => _lang == 0 ? $"Сгенерирован патч: {name}" : _lang == 1 ? $"Generated patch: {name}" : $"已生成补丁: {name}";
    public string PatchRowsLabel              => _lang == 0 ? "строк" : _lang == 1 ? "rows" : "行";
    public string PatchTotalLabel(int total)    => _lang == 0 ? $"Итого строк: {total}" : _lang == 1 ? $"Total rows: {total}" : $"总行数: {total}";
    public string PatchErrorTitle             => T("Ошибка генерации патча", "Patch generation error", "补丁生成错误");
    public string PatchErrorMsg               => T("Ошибка генерации патча", "Error generating patch", "生成补丁时出错");
    public string StatusImportDone(int ins, int upd) =>
        _lang == 0 ? $"Импорт завершён. Добавлено: {ins}, обновлено: {upd}"
            : _lang == 1 ? $"Import done. Inserted: {ins}, updated: {upd}"
            : $"导入完成. 已插入: {ins}, 已更新: {upd}";
    public string StatusImportDbDone(int ins, int upd) =>
        _lang == 0 ? $"Импорт из БД завершён. Добавлено: {ins}, обновлено: {upd}"
            : _lang == 1 ? $"DB import done. Inserted: {ins}, updated: {upd}"
            : $"数据库导入完成. 已插入: {ins}, 已更新: {upd}";

    // ── Extract NPC Drops ─────────────────────────────────────────────────────

    public string ExtractNpcTitle          => T("Сохранить данные дропа NPC", "Save NPC Drop Data", "保存NPC掉落数据");
    public string ExtractNpcFilter         => T("JSON (*.json)|*.json|Все файлы|*.*", "JSON (*.json)|*.json|All files|*.*", "JSON (*.json)|*.json|所有文件|*.*");
    public string ExtractNpcDefaultFile    => "npc_drops.json";
    public string ExtractNpcNoDbWarning    => T("Сначала откройте базу данных лута.", "Please open loot database first.", "请先打开掉落数据库。");
    public string ExtractNpcNoDataInfo     => T("В базе данных не найдены записи о дропе NPC.", "No NPC drop records found in database.", "数据库中未找到NPC掉落记录。");
    public string ExtractNpcNoDataStatus   => T("Извлечение завершено: данные не найдены", "Extraction complete: no data found", "提取完成：未找到数据");
    public string ExtractNpcSuccess(string fileName, int count) =>
        _lang == 0 ? $"Успешно экспортированы данные дропа {count} NPC в {fileName}"
            : _lang == 1 ? $"Successfully exported drop data for {count} NPCs to {fileName}"
            : $"已成功导出{count}个NPC的掉落数据到{fileName}";
    public string ExtractNpcSuccessTitle   => T("Успех", "Success", "成功");
    public string ExtractNpcErrorTitle     => T("Ошибка экспорта", "Export Error", "导出错误");
    public string ExtractNpcErrorMsg       => T("Произошла ошибка при экспорте", "An error occurred during export", "导出时出错");
    public string ExtractNpcFailedStatus   => T("Экспорт не удался", "Export failed", "导出失败");
    public string ExtractNpcWarningTitle   => T("Предупреждение", "Warning", "警告");
    public string ExtractNpcInfoTitle      => T("Информация", "Info", "信息");

    // ── Common Dialog Titles ──────────────────────────────────────────────────

    public string DialogTitleWarning       => T("Предупреждение", "Warning", "警告");
    public string DialogTitleError         => T("Ошибка", "Error", "错误");
    public string DialogTitleInfo          => T("Информация", "Information", "信息");
    public string DialogTitleSuccess       => T("Успех", "Success", "成功");
    public string DialogTitleQuestion      => T("Вопрос", "Question", "询问");
    public string DialogTitleNoDb          => T("Нет базы", "No Database", "没有数据库");
    public string DialogTitleNoLootData    => T("Нет данных лута", "No Loot Data", "没有战利品数据");
    public string DialogTitleWriteToDb     => T("Запись в БД", "Write to DB", "写入数据库");
    public string DialogTitleImportSql     => T("Импорт SQL", "SQL Import", "SQL导入");
    public string DialogTitleApplyFromDb   => T("Применить из БД", "Apply from DB", "从数据库应用");
    public string DialogTitleUndoDbApply   => T("Отменить из БД", "Undo DB Apply", "撤销数据库应用");
    public string DialogTitleNothingToWrite => T("Нечего записывать", "Nothing to Write", "没有可写入的内容");
    public string DialogTitleDone          => T("Готово", "Done", "完成");

    // ── Open File Dialogs ─────────────────────────────────────────────────────

    public string OpenDbDialogTitle        => T("Выбери SQLite базу (предметы, NPC, категории)", "Select SQLite DB (items, NPCs, categories)", "选择SQLite数据库 (物品、NPC、分类)");
    public string OpenDbDialogFilter       => T("SQLite (*.sqlite3;*.db)|*.sqlite3;*.db|Все файлы|*.*", "SQLite (*.sqlite3;*.db)|*.sqlite3;*.db|All files|*.*", "SQLite (*.sqlite3;*.db)|*.sqlite3;*.db|所有文件|*.*");
    public string OpenLootDbDialogTitle    => T("Выбери SQLite базу лута (loots, loot_groups, loot_pack_dropping_npcs)", "Select SQLite loot DB (loots, loot_groups, loot_pack_dropping_npcs)", "选择SQLite战利品数据库 (loots, loot_groups, loot_pack_dropping_npcs)");
    public string OpenLootDbDialogFilter   => T("SQLite (*.sqlite3;*.db)|*.sqlite3;*.db|Все файлы|*.*", "SQLite (*.sqlite3;*.db)|*.sqlite3;*.db|All files|*.*", "SQLite (*.sqlite3;*.db)|*.sqlite3;*.db|所有文件|*.*");
    public string OpenJsonDialogTitle      => T("Выбери JSON файл(ы) дропа", "Select drop JSON file(s)", "选择掉落JSON文件");
    public string OpenJsonDialogFilter     => T("JSON (*.json)|*.json|Все файлы|*.*", "JSON (*.json)|*.json|All files|*.*", "JSON (*.json)|*.json|所有文件|*.*");
    public string OpenSqlDialogTitle       => T("Выбери SQL файл(ы) для импорта", "Select SQL file(s) for import", "选择要导入的SQL文件");
    public string OpenSqlDialogFilter      => T("SQL files (*.sql)|*.sql|All files (*.*)|*.*", "SQL files (*.sql)|*.sql|All files (*.*)|*.*", "SQL文件 (*.sql)|*.sql|所有文件 (*.*)|*.*");
    public string SaveResultDialogTitle    => T("Сохранить результат", "Save Result", "保存结果");
    public string SaveResultDialogFilter   => T("JSON (*.json)|*.json|Все файлы|*.*", "JSON (*.json)|*.json|All files|*.*", "JSON (*.json)|*.json|所有文件|*.*");

    // ── MessageBox Texts ──────────────────────────────────────────────────────

    public string MsgNoDbSelected          => T("Сначала выбери SQLite базу.", "Please select SQLite database first.", "请先选择SQLite数据库。");
    public string MsgLoadError             => T("Ошибка загрузки", "Load error", "加载错误");
    public string MsgNoUnprocessedItems    => T("Необработанных предметов больше нет.", "No more unprocessed items.", "没有更多未处理的物品。");
    public string MsgSaveError             => T("Ошибка сохранения", "Save error", "保存错误");
    public string MsgExportError           => T("Ошибка экспорта", "Export error", "导出错误");
    public string MsgDbAnalysisError       => T("Ошибка при анализе БД", "Database analysis error", "数据库分析错误");
    public string MsgSyncConfirm(string toUpdate, string toInsert, string lootGroupRows) =>
        _lang == 0 ? $"Будет обновлено:\n  loots:       обновить {toUpdate}, добавить {toInsert}\n  loot_groups: обновить {lootGroupRows}\n\nПродолжить?"
            : _lang == 1 ? $"Will update:\n  loots:       update {toUpdate}, insert {toInsert}\n  loot_groups: update {lootGroupRows}\n\nContinue?"
            : $"将更新:\n  loots:       更新 {toUpdate}, 插入 {toInsert}\n  loot_groups: 更新 {lootGroupRows}\n\n是否继续?";
    public string MsgWriteDbError          => T("Ошибка записи в БД", "Database write error", "数据库写入错误");
    public string MsgImportSqlSuccess(int fileCount) =>
        _lang == 0 ? $"Импорт завершён успешно ({fileCount} файл(а/ов))."
            : _lang == 1 ? $"Import completed successfully ({fileCount} file(s))."
            : $"导入成功完成 ({fileCount} 个文件)。";
    public string MsgImportSqlAdded        => T("Добавлено", "Added", "已添加");
    public string MsgImportSqlUpdated      => T("Обновлено", "Updated", "已更新");
    public string MsgImportSqlOther        => T("Прочих", "Other", "其他");
    public string MsgImportSqlTotal        => T("Итого", "Total", "总计");
    public string MsgImportSqlError        => T("Ошибка импорта SQL", "SQL import error", "SQL导入错误");
    public string MsgLootTablesMissing     => T("Лут-таблицы (loots, loot_pack_dropping_npcs) не найдены в основной базе.\n\n" +
                                                "Если они находятся в отдельном файле (например compact.server.table.sqlite3),\n" +
                                                "нажми кнопку «Открыть БД лута» на панели инструментов.",
                                                "Loot tables (loots, loot_pack_dropping_npcs) not found in main database.\n\n" +
                                                "If they are in a separate file (e.g., compact.server.table.sqlite3),\n" +
                                                "click the 'Open Loot DB' button on the toolbar.",
                                                "主数据库中未找到战利品表 (loots, loot_pack_dropping_npcs)。\n\n" +
                                                "如果它们在单独的文件中 (例如 compact.server.table.sqlite3)，\n" +
                                                "请点击工具栏上的'打开战利品数据库'按钮。");
    public string MsgNoItemsWithDbData     => T("Нет предметов с данными из БД, у которых ещё не задана группа предмета.",
                                                "No items with DB data that don't have an item group set yet.",
                                                "没有尚未设置物品组的数据库数据物品。");
    public string MsgApplyFromDbConfirm(int count) =>
        _lang == 0 ? $"Применить Группу (БД) и Шанс (БД) как item-level значения для {count} предмет(ов)?"
            : _lang == 1 ? $"Apply Group (DB) and Chance (DB) as item-level values for {count} item(s)?"
            : $"将组 (数据库) 和几率 (数据库) 作为物品级值应用于 {count} 个物品?";
    public string MsgGroupMustBeInteger    => T("Группа должна быть целым числом.", "Group must be an integer.", "组必须是整数。");
    public string MsgChanceMustBeNumber    => T("Шанс должен быть числом.", "Chance must be a number.", "几率必须是数字。");
    public string MsgSkippedFiles(string files) =>
        _lang == 0 ? $"Пропущены файлы (не является массивом JSON):\n{files}"
            : _lang == 1 ? $"Skipped files (not a JSON array):\n{files}"
            : $"跳过的文件 (不是JSON数组):\n{files}";
    public string MsgJsonLoaded(int items, int npcs) =>
        _lang == 0 ? $"JSON: {items} предметов, {npcs} NPC. Загружаем имена…"
            : _lang == 1 ? $"JSON: {items} items, {npcs} NPCs. Loading names…"
            : $"JSON: {items} 个物品, {npcs} 个NPC. 正在加载名称…";
    public string MsgNoItemsWithAssignedGroup => T("Нет предметов с назначенными группой и шансом.",
                                                    "No items with assigned group and chance.",
                                                    "没有已分配组和几率的物品。");
}
