using System.IO;
using System.Text.Json;

namespace LootMaster.Services;

/// <summary>
/// Parses NPC-loot and doodad-loot JSON files.
///
/// Supported formats:
///   NPC:    [ { "npc_id": 123, "items": [ { "item_id": 456 } ] } ]
///   Doodad: [ { "doodad_id": 322, "loot_pack_id": 6414, "items": [ { "item_id": 456 } ] } ]
///
/// Doodad IDs are stored in the same source maps as NPC IDs.
/// loot_pack_id from doodad entries is collected into DoodadToLootPack so
/// DatabaseService can use it without querying loot_pack_dropping_npcs.
/// </summary>
public static class JsonSourceParser
{
    public record ParseResult(
        Dictionary<int, HashSet<int>> ItemToNpcs,
        Dictionary<int, HashSet<int>> NpcToItems,
        Dictionary<int, int> DoodadToLootPack,
        IReadOnlyList<string> SkippedFiles);

    public static async Task<ParseResult> ParseAsync(IEnumerable<string> paths)
    {
        Dictionary<int, HashSet<int>> itemToNpcs = [];
        Dictionary<int, HashSet<int>> npcToItems = [];
        Dictionary<int, int> doodadToLootPack = [];
        List<string> skipped = [];

        foreach (var path in paths)
        {
            await using var stream = File.OpenRead(path);
            using var doc = await JsonDocument.ParseAsync(stream);

            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                skipped.Add(Path.GetFileName(path));
                continue;
            }

            foreach (var entry in doc.RootElement.EnumerateArray())
            {
                int sourceId;

                if (entry.TryGetProperty("npc_id", out var npcProp))
                {
                    sourceId = npcProp.GetInt32();
                }
                else if (entry.TryGetProperty("doodad_id", out var doodadProp))
                {
                    sourceId = doodadProp.GetInt32();

                    // Collect loot_pack_id directly from doodad entry
                    if (entry.TryGetProperty("loot_pack_id", out var packProp))
                        doodadToLootPack.TryAdd(sourceId, packProp.GetInt32());
                }
                else continue;

                if (!entry.TryGetProperty("items", out var itemsProp) ||
                    itemsProp.ValueKind != JsonValueKind.Array) continue;

                npcToItems.TryAdd(sourceId, []);

                foreach (var item in itemsProp.EnumerateArray())
                {
                    if (!item.TryGetProperty("item_id", out var itemProp)) continue;
                    int itemId = itemProp.GetInt32();

                    if (!itemToNpcs.TryGetValue(itemId, out var npcSet))
                    {
                        npcSet = [];
                        itemToNpcs[itemId] = npcSet;
                    }
                    npcSet.Add(sourceId);
                    npcToItems[sourceId].Add(itemId);
                }
            }
        }

        return new ParseResult(itemToNpcs, npcToItems, doodadToLootPack, skipped);
    }
}
