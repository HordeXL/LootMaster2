using System.Text.Json.Serialization;

namespace LootMaster.Models;

public class ItemProgress
{
    [JsonPropertyName("group")]
    public int? Group { get; set; }

    [JsonPropertyName("chance")]
    public double? Chance { get; set; }

    [JsonPropertyName("min_amount")]
    public int? MinAmount { get; set; }

    [JsonPropertyName("max_amount")]
    public int? MaxAmount { get; set; }

    [JsonPropertyName("grade_id")]
    public int? GradeId { get; set; }

    [JsonPropertyName("always_drop")]
    public bool? AlwaysDrop { get; set; }
}
