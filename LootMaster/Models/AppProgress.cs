using System.Text.Json.Serialization;

namespace LootMaster.Models;

public class AppProgress
{
    [JsonPropertyName("db_path")]
    public string DbPath { get; set; } = "";

    /// <summary>Path to the server.table DB with loot tables (loots, loot_groups, loot_pack_dropping_npcs).
    /// If empty, DbPath is used for loot tables as well.</summary>
    [JsonPropertyName("loot_db_path")]
    public string LootDbPath { get; set; } = "";

    [JsonPropertyName("source_json_paths")]
    public List<string> SourceJsonPaths { get; set; } = new();

    /// <summary>Legacy single-path field — kept for backward-compat with Python tool's progress files.</summary>
    [JsonPropertyName("source_json_path")]
    public string? SourceJsonPath { get; set; }

    /// <summary>item_id (string) → ItemProgress</summary>
    [JsonPropertyName("items")]
    public Dictionary<string, ItemProgress> Items { get; set; } = new();

    /// <summary>category_id (string) → CategoryProgress</summary>
    [JsonPropertyName("categories")]
    public Dictionary<string, CategoryProgress> Categories { get; set; } = new();
}
