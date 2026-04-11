# LootMaster 核心技术实现文档

本文档详细阐述了 LootMaster 项目中四个核心功能的技术细节与代码逻辑，旨在为后续开发和维护提供清晰的技术参考。

---

## 1. NPC 掉落数据提取功能

该功能允许用户将数据库中复杂的 NPC 掉落关系（涉及 `loot_pack_dropping_npcs` 和 `loots` 两张表）提取并转换为结构化的 JSON 文件。

### 1.1 实现流程
1.  **映射构建**：首先查询 `loot_pack_dropping_npcs` 表，建立 `npc_id` 到 `loot_pack_id` 集合的映射。一个 NPC 可能关联多个掉落包。
2.  **物品聚合**：遍历所有收集到的 `loot_pack_id`，从 `loots` 表中批量查询对应的 `item_id`。
3.  **数据重组**：将物品 ID 聚合回对应的 NPC ID 下，形成嵌套结构。
4.  **序列化导出**：使用 `System.Text.Json` 将结果序列化为蛇形命名（snake_case）的 JSON 格式。

### 1.2 关键代码逻辑 (`DatabaseService.cs`)
```csharp
public async Task<List<NpcDropRecord>> ExtractNpcDropsAsync(...)
{
    // 1. 加载 npc_id -> loot_pack_id 映射
    var npcToPacks = new Dictionary<int, HashSet<int>>();
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = "SELECT npc_id, loot_pack_id FROM loot_pack_dropping_npcs";
        // ... 读取数据填充字典
    }

    // 2. 批量加载所有相关掉落包的物品
    var packToItems = new Dictionary<int, HashSet<int>>();
    foreach (var chunk in Chunked(allPackIds, 500)) // 分块处理避免 SQL 变量超限
    {
        // SELECT loot_pack_id, item_id FROM loots WHERE loot_pack_id IN (...)
        // ... 填充 packToItems
    }

    // 3. 按 NPC 聚合并排序
    return npcToPacks.Select(kvp => new NpcDropRecord {
        NpcId = kvp.Key,
        Items = kvp.Value.SelectMany(packId => packToItems[packId])
                         .Distinct().OrderBy(id => id)
                         .Select(id => new ItemDropRecord { ItemId = id })
                         .ToList()
    }).ToList();
}
```

---

## 2. 数据库中文本地化支持

LootMaster 通过动态 SQL 构造实现了对俄语 (RU)、英语 (EN) 和简体中文 (ZH) 的原生支持。

### 2.1 动态字段选择
在 `GetLocalizedMap` 方法中，根据传入的 `language` 参数（0=RU, 1=EN, 2=ZH），动态决定 `SELECT` 语句中的主选字段和备选字段。

### 2.2 优先级回退逻辑
*   **RU**: 优先取 `ru`，若无则取 `en_us`。
*   **EN**: 优先取 `en_us`，若无则取 `ru`。
*   **ZH**: 优先取 `zh_cn`，若无则取 `en_us`。

### 2.3 关键代码片段 (`DatabaseService.cs`)
```csharp
private static Dictionary<int, string> GetLocalizedMap(..., int language = 0)
{
    // 根据语言模式动态生成列名
    string selectColumns = language switch
    {
        2 => "idx, zh_cn, en_us",      // ZH: 中文 -> 英文
        1 => "idx, en_us, ru",          // EN: 英文 -> 俄文
        _ => "idx, ru, en_us"            // RU: 俄文 -> 英文
    };

    string sql = $"SELECT {selectColumns} FROM localized_texts WHERE ...";
    
    // 读取时：primary (列1) 为空则尝试 secondary (列2)
    result[idx] = !string.IsNullOrEmpty(primary) ? primary : (secondary ?? "");
}
```

---

## 3. JSON 格式修复与解析

`JsonSourceParser.cs` 负责处理两种不同结构的输入文件，并具备容错能力。

### 3.1 多格式兼容
解析器通过检查 JSON 对象的属性名来自动识别格式：
*   **NPC 格式**: 识别 `npc_id` 属性。
*   **Doodad 格式**: 识别 `doodad_id` 属性，并额外提取 `loot_pack_id`。

### 3.2 异常处理与非标准格式
*   **数组校验**: 如果根节点不是 JSON 数组（例如误选了配置文件），该文件会被加入 `SkippedFiles` 列表并弹出警告。
*   **健壮性**: 使用 `TryGetProperty` 确保在缺少关键字段时不会崩溃，而是跳过该条目。

### 3.3 关键代码片段 (`JsonSourceParser.cs`)
```csharp
foreach (var entry in doc.RootElement.EnumerateArray())
{
    int sourceId;
    if (entry.TryGetProperty("npc_id", out var npcProp))
        sourceId = npcProp.GetInt32();
    else if (entry.TryGetProperty("doodad_id", out var doodadProp))
    {
        sourceId = doodadProp.GetInt32();
        // 提取 Doodad 特有的掉落包 ID
        if (entry.TryGetProperty("loot_pack_id", out var packProp))
            doodadToLootPack.TryAdd(sourceId, packProp.GetInt32());
    }
    else continue; // 跳过无法识别的条目
}
```

---

## 4. UI 语言切换机制

项目采用“状态驱动 + 事件通知”的模式实现无刷新语言切换。

### 4.1 模块交互关系
1.  **MainViewModel**: 维护 `_language` 状态（int 类型）。当状态改变时，实例化新的 `UIStrings` 对象并触发 `PropertyChanged`。
2.  **UIStrings**: 纯数据类，根据构造函数传入的语言索引返回对应的字符串。
3.  **ColumnSettingsService**: 负责将语言索引持久化到 `column-settings.json`。
4.  **MainWindow (Code-behind)**: 监听 `LanguageChanged` 事件，手动更新 DataGrid 的列头文本。

### 4.2 切换流程
1.  用户点击工具栏按钮 -> 触发 `ToggleLanguageCommand`。
2.  ViewModel 执行 `_language = (_language + 1) % 3`。
3.  调用 `ApplyLanguageChange()`：
    *   更新 `_ui` 实例。
    *   调用 `_colSvc.SaveLanguage(_language)` 保存偏好。
    *   触发 `OnPropertyChanged(nameof(UI))` 刷新所有绑定文本。
    *   若已加载数据，重新调用 `LoadDataAsync` 以从数据库获取新语言的名称。

### 4.3 关键代码片段 (`MainViewModel.cs`)
```csharp
private void ToggleLanguage()
{
    _language = (_language + 1) % 3; // 循环切换 0->1->2->0
    ApplyLanguageChange();
}

private void ApplyLanguageChange()
{
    _ui = new UIStrings(_language);
    _colSvc.SaveLanguage(_language);
    
    // 通知 UI 更新绑定的字符串
    OnPropertyChanged(nameof(UI));
    RefreshSummary();
    
    // 通知 Code-behind 更新 DataGrid 列头
    LanguageChanged?.Invoke();

    // 重新加载数据以更新物品/NPC 名称
    if (_itemsData.Count > 0)
        _ = LoadDataAsync(_progress.DbPath, _progress.SourceJsonPaths);
}
```

---

## 总结

LootMaster 通过以下设计保证了功能的稳健性与扩展性：
*   **解耦设计**: 数据库查询、JSON 解析与 UI 逻辑完全分离。
*   **动态本地化**: 不依赖硬编码的资源文件，而是直接利用数据库的多语言字段，确保了游戏内名称的实时准确性。
*   **状态持久化**: 用户的语言偏好和界面布局均能自动保存，提升了用户体验。
