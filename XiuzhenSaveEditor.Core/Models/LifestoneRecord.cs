namespace XiuzhenSaveEditor.Models;

/// <summary>
/// Represents a lifestone entry from the svzb.dat file.
/// Each logical record is backed by c2array groups, each containing two sub-values.
/// </summary>
public class LifestoneRecord
{
    /// <summary>
    /// Raw c2array groups. Each entry is a pair [sub0, sub1].
    /// </summary>
    public List<List<C2Value>> Groups { get; set; } = new();

    public string LifestoneId => GetSub(0, 1);
    public string Level       => GetSub(1, 1);
    public string Effect1Id   => GetSub(3, 1);
    public string Effect2Id   => GetSub(4, 1);
    public string Effect3Id   => GetSub(5, 1);
    public string Effect4Id   => GetSub(6, 1);
    public string Effect5Id   => GetSub(8, 1);
    public string Effect1Pct  => GetSub(9, 1);
    public string Effect2Pct  => GetSub(10, 1);
    public string Effect3Pct  => GetSub(11, 1);
    public string Effect4Pct  => GetSub(12, 1);
    public string Effect5Pct  => GetSub(13, 1);

    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(LifestoneId) || LifestoneId == "0";

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
