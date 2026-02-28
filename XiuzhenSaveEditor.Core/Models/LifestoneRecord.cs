namespace XiuzhenSaveEditor.Models;

/// <summary>
/// Represents a lifestone entry from the svzb.dat file.
/// Each line holds 14 bracket groups, each containing 2 comma-separated sub-values.
///
/// Format per line:
///   [?,LID],[?,?],[0,LVL],[0,EID1],[0,EID2],[0,EID3],[0,EID4],[0,?],[0,EID5],
///   [0,EP1],[0,EP2],[0,EP3],[0,EP4],[0,EP5]
///
/// LID  = Lifestone ID
/// LVL  = Level
/// EIDx = Effect ID (1-5)
/// EPx  = Effect Percentage (1-5)
/// </summary>
public class LifestoneRecord
{
    /// <summary>
    /// Raw bracket groups. Each entry is a pair [sub0, sub1].
    /// </summary>
    public List<List<string>> Groups { get; set; } = new();

    public string LifestoneId => GetSub(0, 1);
    public string Level       => GetSub(2, 1);
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
