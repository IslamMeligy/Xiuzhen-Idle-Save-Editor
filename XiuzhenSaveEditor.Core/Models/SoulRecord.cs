namespace XiuzhenSaveEditor.Models;

/// <summary>
/// Represents a soul/farm entry from the svlz.dat file.
/// Each line holds 8 bracket groups, each containing 3 comma-separated sub-values.
///
/// Format per line:
///   [SID,?,0],[SQ,?,FID],[ST1,?,LSU],[ST2,?,LS],[ST3,0,FA],[ST4,0,"?"],[ST5,0,"HN"],[0,0,"?"]
///
/// SID  = Soul ID
/// SQ   = Soul Quantity
/// FID  = Fruit ID
/// ST1  = Spirit stat
/// ST2  = Resonance stat
/// ST3  = Strength stat
/// ST4  = Stability stat
/// ST5  = Fortune stat
/// LSU  = Lifespan Used (months)
/// LS   = Lifespan (months)
/// FA   = Fruit Age (months)
/// HN   = Harvested fruit name
/// </summary>
public class SoulRecord
{
    /// <summary>
    /// Raw bracket groups. Each entry is a list of sub-values within one bracket group.
    /// </summary>
    public List<List<string>> Groups { get; set; } = new();

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
