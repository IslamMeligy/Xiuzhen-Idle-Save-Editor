namespace XiuzhenSaveEditor.Models;

/// <summary>
/// Represents an inventory entry from the svwp.dat file.
/// Each logical record is backed by c2array groups, and the editor currently uses
/// the first two groups as [ItemID] and [Quantity].
/// </summary>
public class InventoryRecord
{
    /// <summary>
    /// Raw c2array groups. Each entry is a list of sub-values within one group.
    /// </summary>
    public List<List<C2Value>> Groups { get; set; } = new();

    public string ItemId   => GetSub(0, 0);
    public string Quantity => GetSub(1, 0);

    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(ItemId) || ItemId == "0";

    private string GetSub(int group, int sub)
    {
        if (group < Groups.Count && sub < Groups[group].Count)
            return Groups[group][sub].Text;
        return "";
    }

    public void SetSub(int group, int sub, string value)
    {
        while (Groups.Count <= group)
            Groups.Add(new List<C2Value>());
        while (Groups[group].Count <= sub)
            Groups[group].Add(C2Value.CreateNumber());
        Groups[group][sub].SetText(value);
    }

    public string ToFileLine()
    {
        var parts = Groups.Select(g => string.Join(",", g.Select(v => v.Text)));
        return "[" + string.Join("],[", parts) + "]";
    }
}
