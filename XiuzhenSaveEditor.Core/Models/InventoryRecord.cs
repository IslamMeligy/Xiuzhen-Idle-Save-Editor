namespace XiuzhenSaveEditor.Models;

/// <summary>
/// Represents an inventory entry from the svwp.dat file.
/// Each line holds bracket groups with comma-separated sub-values.
///
/// Observed format per line:
///   [ItemID,Quantity]
///
/// ItemID = Item ID matching Items.json
/// Quantity = Number of items held
/// </summary>
public class InventoryRecord
{
    /// <summary>
    /// Raw bracket groups. Each entry is a list of sub-values within one bracket group.
    /// </summary>
    public List<List<string>> Groups { get; set; } = new();

    public string ItemId   => GetSub(0, 0);
    public string Quantity => GetSub(0, 1);

    private string GetSub(int group, int sub)
    {
        if (group < Groups.Count && sub < Groups[group].Count)
            return Groups[group][sub];
        return "";
    }

    public void SetSub(int group, int sub, string value)
    {
        while (Groups.Count <= group)
            Groups.Add(new List<string>());
        while (Groups[group].Count <= sub)
            Groups[group].Add("0");
        Groups[group][sub] = value;
    }

    public string ToFileLine()
    {
        var parts = Groups.Select(g => string.Join(",", g));
        return "[" + string.Join("],[", parts) + "]";
    }
}
