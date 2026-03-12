using XiuzhenSaveEditor.Models;

namespace XiuzhenSaveEditor.Parsers;

/// <summary>
/// Parses and serializes the svlz.dat (farm + souls) file.
///
/// The file is stored as a Construct c2array with one soul slot per outer row.
/// </summary>
public static class SoulParser
{
    public static List<SoulRecord> Parse(string filePath)
    {
        var data = C2ArrayParser.Parse(filePath);
        var souls = new List<SoulRecord>(data.Count);

        foreach (List<List<C2Value>> row in data)
        {
            souls.Add(new SoulRecord { Groups = row });
        }

        return souls;
    }

    public static void Save(string filePath, List<SoulRecord> souls)
    {
        C2ArrayParser.Save(filePath, souls.Select(s => (IReadOnlyList<IReadOnlyList<C2Value>>)s.Groups).ToList());
    }
}
