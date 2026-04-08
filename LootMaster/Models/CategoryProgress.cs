using System.Text.Json.Serialization;

namespace LootMaster.Models;

public class CategoryProgress
{
    [JsonPropertyName("group")]
    public int? Group { get; set; }

    [JsonPropertyName("chance")]
    public double? Chance { get; set; }
}
