using System.Text.Json.Serialization;

namespace LootMaster.Models;

public class CategoryProgress
{
    [JsonPropertyName("group")]
    public int? Group { get; set; }

    [JsonPropertyName("chance")]
    public double? Chance { get; set; }

    [JsonPropertyName("grade_id")]
    public int? GradeId { get; set; }

    [JsonPropertyName("always_drop")]
    public bool? AlwaysDrop { get; set; }
}
