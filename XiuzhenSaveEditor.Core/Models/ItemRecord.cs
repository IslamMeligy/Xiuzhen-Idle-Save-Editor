namespace XiuzhenSaveEditor.Models;

/// <summary>
/// Represents a single item from Items.json (c2array format).
/// Column mapping: 0=ID, 1=Name, 2=Category, 3=Tier, 4=SubID,
///                 5=StackFlag, 6=Value, 7=DisplayName, 8=Description
/// </summary>
public class ItemRecord
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public int Tier { get; set; }
    public int SubId { get; set; }
    public int StackFlag { get; set; }
    public double Value { get; set; }
    public string DisplayName { get; set; } = "";
    public string Description { get; set; } = "";
}
