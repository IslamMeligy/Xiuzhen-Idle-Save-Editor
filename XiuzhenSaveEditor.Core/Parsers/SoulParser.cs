using XiuzhenSaveEditor.Models;

namespace XiuzhenSaveEditor.Parsers;

/// <summary>
/// Parses and serializes the svlz.dat (farm + souls) file.
///
/// Each line contains 8 bracket groups with 3 comma-separated sub-values each:
///   [SID,?,0],[SQ,?,FID],[ST1,?,LSU],[ST2,?,LS],[ST3,0,FA],[ST4,0,"?"],[ST5,0,"HN"],[0,0,"?"]
///
/// ST1=Spirit, ST2=Resonance, ST3=Strength, ST4=Stability, ST5=Fortune
/// </summary>
public static class SoulParser
{
    public static List<SoulRecord> Parse(string filePath)
    {
        var souls = new List<SoulRecord>();
        if (!File.Exists(filePath)) return souls;

        foreach (string rawLine in File.ReadAllLines(filePath))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var groups = BracketParser.ExtractGroups(line);
            if (groups.Count == 0) continue;

            var record = new SoulRecord();
            foreach (string g in groups)
            {
                record.Groups.Add(BracketParser.SplitGroup(g));
            }
            souls.Add(record);
        }

        return souls;
    }

    public static void Save(string filePath, List<SoulRecord> souls)
    {
        var lines = souls.Select(s => s.ToFileLine()).ToList();
        File.WriteAllLines(filePath, lines);
    }
}
