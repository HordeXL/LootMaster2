using LootMaster.Models;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LootMaster.Services;

/// <summary>
/// Handles persistence of the loot-assignment progress file and JSON export.
/// Compatible with the progress format used by the Python lott parser.py tool.
/// </summary>
public sealed class ProgressService(string stateFilePath)
{
    private static readonly JsonSerializerOptions _opts = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    // ──────────────────────────────────────────────────────────────────────────
    // Load / Save
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<AppProgress?> LoadAsync()
    {
        if (!File.Exists(stateFilePath)) return null;
        await using var stream = File.OpenRead(stateFilePath);
        var progress = await JsonSerializer.DeserializeAsync<AppProgress>(stream, _opts);

        // Migrate legacy single-path field from Python tool
        if (progress is not null
            && !string.IsNullOrEmpty(progress.SourceJsonPath)
            && progress.SourceJsonPaths.Count == 0)
        {
            progress.SourceJsonPaths.Add(progress.SourceJsonPath);
        }

        return progress;
    }

    public async Task SaveAsync(AppProgress progress)
    {
        string dir = Path.GetDirectoryName(stateFilePath)!;
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        await using var stream = File.Create(stateFilePath);
        await JsonSerializer.SerializeAsync(stream, progress, _opts);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Export
    // ──────────────────────────────────────────────────────────────────────────

    public async Task ExportAsync(string outputPath, IReadOnlyList<(int ItemId, double? Chance)> items)
    {
        var payload = new
        {
            items = items.Select(x => new { item_id = x.ItemId, chance = x.Chance }).ToList()
        };

        await using var stream = File.Create(outputPath);
        await JsonSerializer.SerializeAsync(stream, payload, _opts);
    }
}
