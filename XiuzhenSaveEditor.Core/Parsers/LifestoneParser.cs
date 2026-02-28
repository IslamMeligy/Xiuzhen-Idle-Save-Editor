using XiuzhenSaveEditor.Models;

namespace XiuzhenSaveEditor.Parsers;

/// <summary>
/// Parses and serializes the svzb.dat (lifestones) file.
///
/// Each line contains 14 bracket groups with 2 comma-separated sub-values each:
///   [?,LID],[?,?],[0,LVL],[0,EID1],[0,EID2],[0,EID3],[0,EID4],[0,?],[0,EID5],
///   [0,EP1],[0,EP2],[0,EP3],[0,EP4],[0,EP5]
///
/// LID = Lifestone ID, LVL = Level, EIDx = Effect ID, EPx = Effect Percentage
/// </summary>
public static class LifestoneParser
{
    public static List<LifestoneRecord> Parse(string filePath)
    {
        var stones = new List<LifestoneRecord>();
        if (!File.Exists(filePath)) return stones;

        foreach (string rawLine in File.ReadAllLines(filePath))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var groups = BracketParser.ExtractGroups(line);
            if (groups.Count == 0) continue;

            var record = new LifestoneRecord();
            foreach (string g in groups)
            {
                record.Groups.Add(BracketParser.SplitGroup(g));
            }
            stones.Add(record);
        }

        return stones;
    }

    public static void Save(string filePath, List<LifestoneRecord> stones)
    {
        var lines = stones.Select(s => s.ToFileLine()).ToList();
        File.WriteAllLines(filePath, lines);
    }
}
