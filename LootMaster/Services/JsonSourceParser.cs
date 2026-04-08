using System.IO;
using System.Text.Json;

namespace LootMaster.Services;

/// <summary>
/// Parses the NPC-loot JSON files produced by the ArcheAge emulator extractor.
/// Supports the format used in loot1.json / loot_doodad.json:
///   [ { "npc_id": 123, "items": [ { "item_id": 456, ... }, ... ] }, ... ]
/// Multiple files can be merged — duplicate relationships are deduplicated.
/// </summary>
public static class JsonSourceParser
{
    public record ParseResult(
        Dictionary<int, HashSet<int>> ItemToNpcs,
        Dictionary<int, HashSet<int>> NpcToItems,
        IReadOnlyList<string> SkippedFiles);

    public static async Task<ParseResult> ParseAsync(IEnumerable<string> paths)
    {
        var itemToNpcs = new Dictionary<int, HashSet<int>>();
        var npcToItems = new Dictionary<int, HashSet<int>>();
        var skipped = new List<string>();

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
                if (!entry.TryGetProperty("npc_id", out var npcProp)) continue;
                int npcId = npcProp.GetInt32();

                if (!entry.TryGetProperty("items", out var itemsProp) ||
                    itemsProp.ValueKind != JsonValueKind.Array) continue;

                npcToItems.TryAdd(npcId, new HashSet<int>());

                foreach (var item in itemsProp.EnumerateArray())
                {
                    if (!item.TryGetProperty("item_id", out var itemProp)) continue;
                    int itemId = itemProp.GetInt32();

                    itemToNpcs.TryGetValue(itemId, out var npcSet);
                    if (npcSet is null) { npcSet = new HashSet<int>(); itemToNpcs[itemId] = npcSet; }
                    npcSet.Add(npcId);
                    npcToItems[npcId].Add(itemId);
                }
            }
        }

        return new ParseResult(itemToNpcs, npcToItems, skipped);
    }
}
