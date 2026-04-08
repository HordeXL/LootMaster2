using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace LootMaster.Services;

/// <summary>
/// Synchronises loot assignments back into the ArcheAge SQLite database.
///
/// Schema relations:
///   loot_pack_dropping_npcs : npc_id → loot_pack_id
///   loot_groups             : pack_id + group_no → drop_rate  (group-level chance)
///   loots                   : loot_pack_id + item_id + group  → drop_rate (item-level chance)
///
/// Strategy:
///   • Only rows reachable via the loaded JSON (npc_id → item_id pairs) are touched.
///   • loot_groups.drop_rate is set to 10 000 000 (100%) for every affected group
///     so that effective item chance equals loots.drop_rate / 10 000 000.
///   • loots.group and loots.drop_rate are upserted (UPDATE if exists, INSERT if not).
///   • Everything runs in a single transaction — all or nothing.
/// </summary>
public sealed class DbSyncService(string dbPath)
{
    private const int MaxDropRate = 10_000_000;

    public record SyncPreview(int ToUpdate, int ToInsert, int LootGroupRows);

    /// <summary>
    /// Calculates how many rows will be affected without writing anything.
    /// </summary>
    public async Task<SyncPreview> PreviewAsync(
        IReadOnlyList<SyncItem> items,
        CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            using var conn = OpenConnection(readOnly: true);
            int toUpdate = 0, toInsert = 0, groupRows = 0;

            foreach (var item in items)
            {
                var packIds = GetPackIds(conn, item.NpcIds);
                foreach (int packId in packIds)
                {
                    bool exists = LootRowExists(conn, packId, item.ItemId);
                    if (exists) toUpdate++; else toInsert++;

                    if (LootGroupRowExists(conn, packId, item.Group)) groupRows++;
                }
            }
            return new SyncPreview(toUpdate, toInsert, groupRows);
        }, ct);
    }

    /// <summary>
    /// Writes all assignments to the database inside a single transaction.
    /// Upserts loots (UPDATE if exists, INSERT if not) and loot_groups.
    /// </summary>
    public async Task SyncAsync(
        IReadOnlyList<SyncItem> items,
        IProgress<string>? progress = null,
        CancellationToken ct = default)
    {
        await Task.Run(() =>
        {
            using var conn = OpenConnection(readOnly: false);
            using var tx = conn.BeginTransaction();

            // Reserve ID ranges upfront to avoid MAX(id) races within the loop
            int nextLootId = GetMaxId(conn, "loots", tx) + 1;
            int nextGroupId = GetMaxId(conn, "loot_groups", tx) + 1;

            int done = 0;
            foreach (var item in items)
            {
                ct.ThrowIfCancellationRequested();
                int dropRate = (int)Math.Round(item.Chance * MaxDropRate / 100.0);

                foreach (int packId in GetPackIds(conn, item.NpcIds, tx))
                {
                    // ── loots upsert ──────────────────────────────────────────
                    if (LootRowExists(conn, packId, item.ItemId, tx))
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.Transaction = tx;
                        cmd.CommandText = """
                            UPDATE loots
                               SET "group"   = @grp,
                                   drop_rate = @rate
                             WHERE loot_pack_id = @pack
                               AND item_id      = @item
                            """;
                        cmd.Parameters.AddWithValue("@grp",  item.Group);
                        cmd.Parameters.AddWithValue("@rate", dropRate);
                        cmd.Parameters.AddWithValue("@pack", packId);
                        cmd.Parameters.AddWithValue("@item", item.ItemId);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.Transaction = tx;
                        cmd.CommandText = """
                            INSERT INTO loots
                                (id, "group", item_id, drop_rate, min_amount, max_amount, loot_pack_id, grade_id, always_drop)
                            VALUES
                                (@id, @grp, @item, @rate, 1, 1, @pack, 0, 'f')
                            """;
                        cmd.Parameters.AddWithValue("@id",   nextLootId++);
                        cmd.Parameters.AddWithValue("@grp",  item.Group);
                        cmd.Parameters.AddWithValue("@item", item.ItemId);
                        cmd.Parameters.AddWithValue("@rate", dropRate);
                        cmd.Parameters.AddWithValue("@pack", packId);
                        cmd.ExecuteNonQuery();
                    }

                    // ── loot_groups upsert ────────────────────────────────────
                    if (LootGroupRowExists(conn, packId, item.Group, tx))
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.Transaction = tx;
                        cmd.CommandText = """
                            UPDATE loot_groups
                               SET drop_rate = @maxRate
                             WHERE pack_id  = @pack
                               AND group_no = @grp
                            """;
                        cmd.Parameters.AddWithValue("@maxRate", MaxDropRate);
                        cmd.Parameters.AddWithValue("@pack",    packId);
                        cmd.Parameters.AddWithValue("@grp",     item.Group);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.Transaction = tx;
                        cmd.CommandText = """
                            INSERT INTO loot_groups
                                (id, pack_id, group_no, drop_rate, item_grade_distribution_id)
                            VALUES
                                (@id, @pack, @grp, @maxRate, 0)
                            """;
                        cmd.Parameters.AddWithValue("@id",      nextGroupId++);
                        cmd.Parameters.AddWithValue("@pack",    packId);
                        cmd.Parameters.AddWithValue("@grp",     item.Group);
                        cmd.Parameters.AddWithValue("@maxRate", MaxDropRate);
                        cmd.ExecuteNonQuery();
                    }
                }

                done++;
                if (done % 50 == 0)
                    progress?.Report($"Записываю в БД… {done}/{items.Count}");
            }

            tx.Commit();
            progress?.Report($"Готово. Записано предметов: {items.Count}");
        }, ct);
    }

    public record ImportResult(int Inserted, int Replaced, int Updated, int Other);

    /// <summary>
    /// Executes all non-empty, non-comment SQL statements from a file in a single transaction.
    /// Returns counts of INSERT / UPDATE / other statements.
    /// </summary>
    public async Task<ImportResult> ImportSqlFileAsync(
        string filePath,
        IProgress<string>? progress = null,
        CancellationToken ct = default)
    {
        var lines = await File.ReadAllLinesAsync(filePath, ct);

        // Collect full statements (join lines, split on ';')
        var statements = new List<string>();
        var current = new System.Text.StringBuilder();
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("--") || trimmed.Length == 0) continue;
            current.Append(' ').Append(trimmed);
            if (trimmed.EndsWith(';'))
            {
                statements.Add(current.ToString().Trim());
                current.Clear();
            }
        }

        return await Task.Run(() =>
        {
            using var conn = OpenConnection(readOnly: false);
            using var tx = conn.BeginTransaction();
            int inserted = 0, replaced = 0, updated = 0, other = 0;
            int done = 0;
            foreach (var rawSql in statements)
            {
                ct.ThrowIfCancellationRequested();

                var parsed = ParseInsert(rawSql);
                if (parsed is not null)
                {
                    var (table, cols, vals) = parsed.Value;
                    int idIdx = Array.FindIndex(cols, c => c.Equals("id", StringComparison.OrdinalIgnoreCase));
                    string idVal = idIdx >= 0 ? vals[idIdx] : null!;

                    bool exists = idVal != null && RowExistsById(conn, tx, table, idVal);
                    if (!exists)
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.Transaction = tx;
                        cmd.CommandText = rawSql;
                        cmd.ExecuteNonQuery();
                        inserted++;
                    }
                    else
                    {
                        // UPDATE без удаления — оригинальный id сохраняется
                        var setClauses = cols
                            .Select((c, i) => i != idIdx ? $"\"{c}\" = {vals[i]}" : null)
                            .Where(x => x != null);
                        using var cmd = conn.CreateCommand();
                        cmd.Transaction = tx;
                        cmd.CommandText = $"UPDATE \"{table}\" SET {string.Join(", ", setClauses)} WHERE id = {idVal}";
                        cmd.ExecuteNonQuery();
                        replaced++;
                    }
                }
                else
                {
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandText = rawSql;
                    cmd.ExecuteNonQuery();
                    if (rawSql.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase)) updated++;
                    else other++;
                }

                done++;
                if (done % 100 == 0)
                    progress?.Report($"Импорт SQL… {done}/{statements.Count}");
            }
            tx.Commit();
            return new ImportResult(inserted, replaced, updated, other);
        }, ct);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private SqliteConnection OpenConnection(bool readOnly)
    {
        var mode = readOnly ? "ReadOnly" : "ReadWrite";
        var conn = new SqliteConnection($"Data Source={dbPath};Mode={mode};");
        conn.Open();
        return conn;
    }

    private static List<int> GetPackIds(SqliteConnection conn, IReadOnlyList<int> npcIds, SqliteTransaction? tx = null)
    {
        if (npcIds.Count == 0) return [];
        var placeholders = string.Join(',', Enumerable.Range(0, npcIds.Count).Select(i => $"@n{i}"));
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = $"SELECT DISTINCT loot_pack_id FROM loot_pack_dropping_npcs WHERE npc_id IN ({placeholders})";
        for (int i = 0; i < npcIds.Count; i++)
            cmd.Parameters.AddWithValue($"@n{i}", npcIds[i]);

        List<int> result = [];
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) result.Add(reader.GetInt32(0));
        return result;
    }

    private static bool LootRowExists(SqliteConnection conn, int packId, int itemId, SqliteTransaction? tx = null)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT COUNT(*) FROM loots WHERE loot_pack_id=@p AND item_id=@i";
        cmd.Parameters.AddWithValue("@p", packId);
        cmd.Parameters.AddWithValue("@i", itemId);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    private static bool LootGroupRowExists(SqliteConnection conn, int packId, int groupNo, SqliteTransaction? tx = null)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT COUNT(*) FROM loot_groups WHERE pack_id=@p AND group_no=@g";
        cmd.Parameters.AddWithValue("@p", packId);
        cmd.Parameters.AddWithValue("@g", groupNo);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    private static bool RowExistsById(SqliteConnection conn, SqliteTransaction tx, string table, string idVal)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = $"SELECT COUNT(*) FROM \"{table}\" WHERE id = {idVal}";
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    private static readonly Regex _insertRx = new(
        @"INSERT\s+INTO\s+(\w+)\s*\(([^)]+)\)\s*VALUES\s*\((.+)\)\s*;?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>Parses INSERT INTO table (cols) VALUES (vals) → (table, cols[], vals[]).</summary>
    private static (string table, string[] cols, string[] vals)? ParseInsert(string sql)
    {
        var m = _insertRx.Match(sql);
        if (!m.Success) return null;

        string table = m.Groups[1].Value;
        string[] cols = m.Groups[2].Value
            .Split(',')
            .Select(c => c.Trim().Trim('"').Trim('\''))
            .ToArray();
        string[] vals = SplitSqlValues(m.Groups[3].Value);
        if (cols.Length != vals.Length) return null;
        return (table, cols, vals);
    }

    /// <summary>Splits SQL VALUES list respecting single-quoted strings.</summary>
    private static string[] SplitSqlValues(string valuesStr)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuote = false;
        foreach (char c in valuesStr)
        {
            if (c == '\'' && !inQuote) { inQuote = true; current.Append(c); }
            else if (c == '\'' && inQuote) { inQuote = false; current.Append(c); }
            else if (c == ',' && !inQuote) { result.Add(current.ToString().Trim()); current.Clear(); }
            else current.Append(c);
        }
        if (current.Length > 0) result.Add(current.ToString().Trim());
        return [.. result];
    }

    private static int GetMaxId(SqliteConnection conn, string table, SqliteTransaction? tx = null)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = $"SELECT COALESCE(MAX(id), 0) FROM \"{table}\"";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}

/// <summary>One item assignment ready to be written to the database.</summary>
public sealed record SyncItem(int ItemId, IReadOnlyList<int> NpcIds, int Group, double Chance);
