namespace XiuzhenSaveEditor.Models;

/// <summary>
/// Represents a soul/farm entry from the svlz.dat file.
/// Each logical record is backed by c2array groups, each containing three sub-values.
/// </summary>
public class SoulRecord
{
    /// <summary>
    /// Raw c2array groups. Each entry is a list of sub-values within one group.
    /// </summary>
    public List<List<C2Value>> Groups { get; set; } = new();

    // Convenience accessors
    public string SoulId       => GetSub(0, 0);
    public string SoulQuantity => GetSub(1, 0);
    public string FruitId      => GetSub(1, 2);
    public string Spirit       => GetSub(2, 0);  // ST1
    public string LifespanUsed => GetSub(2, 2);  // LSU
    public string Resonance    => GetSub(3, 0);  // ST2
    public string Lifespan     => GetSub(3, 2);  // LS
    public string Strength     => GetSub(4, 0);  // ST3
    public string FruitAge     => GetSub(4, 2);  // FA
    public string Stability    => GetSub(5, 0);  // ST4
    public string Fortune      => GetSub(6, 0);  // ST5
    public string HarvestName  => GetSub(6, 2);  // HN

    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(SoulId) || SoulId == "0";

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
            Groups[group].Add(CreateDefaultValue(group, Groups[group].Count));
        Groups[group][sub].SetText(value);
    }

    public string ToFileLine()
    {
        var parts = Groups.Select(g => string.Join(",", g.Select(v => v.Text)));
        return "[" + string.Join("],[", parts) + "]";
    }

    private static C2Value CreateDefaultValue(int group, int sub) =>
        sub == 2 && group >= 5 ? C2Value.CreateString() : C2Value.CreateNumber();
}
