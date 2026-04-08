using LootMaster.Models;
using Microsoft.Data.Sqlite;

namespace LootMaster.Services;

/// <summary>
/// All SQLite queries against the ArcheAge compact.server.table.sqlite3 database.
/// Uses chunked IN clauses (500 per batch) to stay within SQLite variable limits.
/// Localised names: Russian preferred, English fallback — same logic as lott parser.py.
/// </summary>
public sealed class DatabaseService(string dbPath)
{
    private readonly string _connStr = $"Data Source={dbPath};Mode=ReadOnly;";

    // ──────────────────────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<Dictionary<int, ItemInfo>> LoadItemsAsync(
        IReadOnlyList<int> itemIds,
        IReadOnlyDictionary<int, HashSet<int>> itemToNpcs,
        IReadOnlyDictionary<int, string> npcNames,
        IProgress<string>? progress = null,
        CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            using var conn = OpenConnection();

            // 1. Load item rows
            progress?.Report("Загружаю предметы из БД…");
            var itemRows = QueryById(conn, "SELECT id, name, category_id FROM items", itemIds);

            var foundIds = new List<int>(itemRows.Count);
            var categoryIds = new HashSet<int>();
            foreach (var (id, row) in itemRows)
            {
                foundIds.Add(id);
                if (row.TryGetValue("category_id", out var catObj) && catObj is long catId)
                    categoryIds.Add((int)catId);
            }

            // 2. Localised item names
            progress?.Report("Загружаю названия предметов…");
            var itemNames = GetLocalizedMap(conn, foundIds, "items", "name");

            // 3. Load categories
            progress?.Report("Загружаю категории…");
            var catIdList = new List<int>(categoryIds);
            var catRows = QueryById(conn, "SELECT id, name FROM item_categories", catIdList);
            var catNames = GetLocalizedMap(conn, catIdList, "item_categories", "name");

            var categoryNameMap = new Dictionary<int, string>();
            foreach (var (id, row) in catRows)
            {
                string fallback = row.TryGetValue("name", out var n) ? n?.ToString() ?? "" : "";
                categoryNameMap[id] = catNames.TryGetValue(id, out var loc) && !string.IsNullOrEmpty(loc) ? loc : fallback;
            }

            // 4. Build result
            var result = new Dictionary<int, ItemInfo>(foundIds.Count);
            foreach (var id in itemIds)
            {
                if (!itemRows.TryGetValue(id, out var row)) continue;

                int catId = row.TryGetValue("category_id", out var c) && c is long cl ? (int)cl : 0;
                string fallbackName = row.TryGetValue("name", out var nm) ? nm?.ToString() ?? "" : "";
                string locName = itemNames.TryGetValue(id, out var ln) && !string.IsNullOrEmpty(ln) ? ln : fallbackName;

                var npcIds = new List<int>(itemToNpcs.TryGetValue(id, out var set) ? set : []);
                npcIds.Sort();
                var npcNamesList = npcIds.ConvertAll(nId => npcNames.TryGetValue(nId, out var nn) ? nn : "");

                result[id] = new ItemInfo
                {
                    ItemId = id,
                    ItemName = locName,
                    CategoryId = catId,
                    CategoryName = categoryNameMap.TryGetValue(catId, out var cn) ? cn : "",
                    NpcIds = npcIds,
                    NpcNames = npcNamesList,
                };
            }

            return result;
        }, ct);
    }

    public async Task<Dictionary<int, string>> LoadNpcNamesAsync(
        IReadOnlyList<int> npcIds,
        IProgress<string>? progress = null,
        CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            using var conn = OpenConnection();
            progress?.Report("Загружаю NPC из БД…");

            var rows = QueryById(conn, "SELECT id, name FROM npcs", npcIds);
            var locMap = GetLocalizedMap(conn, npcIds, "npcs", "name");

            var result = new Dictionary<int, string>(rows.Count);
            foreach (var (id, row) in rows)
            {
                string fallback = row.TryGetValue("name", out var n) ? n?.ToString() ?? "" : "";
                result[id] = locMap.TryGetValue(id, out var loc) && !string.IsNullOrEmpty(loc) ? loc : fallback;
            }
            return result;
        }, ct);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private SqliteConnection OpenConnection()
    {
        var conn = new SqliteConnection(_connStr);
        conn.Open();
        return conn;
    }

    /// <summary>
    /// SELECT * FROM {baseSql} WHERE id IN (chunk1, chunk2, …)
    /// Returns dict of id → column-value dict.
    /// </summary>
    private static Dictionary<int, Dictionary<string, object?>> QueryById(
        SqliteConnection conn,
        string baseSql,
        IReadOnlyList<int> ids)
    {
        var result = new Dictionary<int, Dictionary<string, object?>>(ids.Count);
        foreach (var chunk in Chunked(ids, 500))
        {
            var placeholders = string.Join(',', System.Linq.Enumerable.Range(0, chunk.Count).Select(i => $"@p{i}"));
            using var cmd = new SqliteCommand($"{baseSql} WHERE id IN ({placeholders})", conn);
            for (int i = 0; i < chunk.Count; i++)
                cmd.Parameters.AddWithValue($"@p{i}", chunk[i]);

            using var reader = cmd.ExecuteReader();
            int fieldCount = reader.FieldCount;
            string[] names = new string[fieldCount];
            for (int i = 0; i < fieldCount; i++) names[i] = reader.GetName(i);

            while (reader.Read())
            {
                var row = new Dictionary<string, object?>(fieldCount);
                int id = reader.GetInt32(0);
                for (int i = 0; i < fieldCount; i++)
                    row[names[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                result[id] = row;
            }
        }
        return result;
    }

    /// <summary>
    /// Queries localized_texts for Russian name, falls back to English.
    /// Identical logic to Python get_localized_map().
    /// </summary>
    private static Dictionary<int, string> GetLocalizedMap(
        SqliteConnection conn,
        IReadOnlyList<int> ids,
        string tblName,
        string tblColumnName)
    {
        var result = new Dictionary<int, string>(ids.Count);
        if (ids.Count == 0) return result;

        foreach (var chunk in Chunked(ids, 500))
        {
            var placeholders = string.Join(',', System.Linq.Enumerable.Range(0, chunk.Count).Select(i => $"@p{i}"));
            string sql = $"""
                SELECT idx, ru, en_us
                FROM localized_texts
                WHERE tbl_name = @tbl
                  AND tbl_column_name = @col
                  AND idx IN ({placeholders})
                ORDER BY id DESC
                """;

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tbl", tblName);
            cmd.Parameters.AddWithValue("@col", tblColumnName);
            for (int i = 0; i < chunk.Count; i++)
                cmd.Parameters.AddWithValue($"@p{i}", chunk[i]);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int idx = reader.GetInt32(0);
                if (result.ContainsKey(idx)) continue; // keep first (ORDER BY id DESC = most recent)

                string? ru = reader.IsDBNull(1) ? null : reader.GetString(1);
                string? en = reader.IsDBNull(2) ? null : reader.GetString(2);
                result[idx] = !string.IsNullOrEmpty(ru) ? ru : (en ?? "");
            }
        }
        return result;
    }

    private static IEnumerable<List<T>> Chunked<T>(IReadOnlyList<T> source, int size)
    {
        for (int i = 0; i < source.Count; i += size)
        {
            int len = Math.Min(size, source.Count - i);
            var chunk = new List<T>(len);
            for (int j = 0; j < len; j++) chunk.Add(source[i + j]);
            yield return chunk;
        }
    }
}
