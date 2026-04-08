namespace LootMaster.Models;

public class ItemInfo
{
    public int ItemId { get; init; }
    public string ItemName { get; init; } = "";
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = "";
    public List<int> NpcIds { get; init; } = new();
    public List<string> NpcNames { get; init; } = new();
}
