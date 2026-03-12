using XiuzhenSaveEditor.Models;

namespace XiuzhenSaveEditor.Parsers;

/// <summary>
/// Parses and serializes the svzb.dat (lifestones) file.
///
/// The file is stored as a Construct c2array with one lifestone slot per outer row.
/// </summary>
public static class LifestoneParser
{
    public static List<LifestoneRecord> Parse(string filePath)
    {
        var data = C2ArrayParser.Parse(filePath);
        var stones = new List<LifestoneRecord>(data.Count);

        foreach (List<List<C2Value>> row in data)
        {
            stones.Add(new LifestoneRecord { Groups = row });
        }

        return stones;
    }

    public static void Save(string filePath, List<LifestoneRecord> stones)
    {
        C2ArrayParser.Save(filePath, stones.Select(s => (IReadOnlyList<IReadOnlyList<C2Value>>)s.Groups).ToList());
    }
}
