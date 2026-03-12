using XiuzhenSaveEditor.Models;

namespace XiuzhenSaveEditor.Parsers;

/// <summary>
/// Parses and serializes the svmp.dat (sect data) file.
///
/// The file is stored as a Construct c2array with nested raw group/subvalue data.
/// </summary>
public static class SectParser
{
    public static List<SectRecord> Parse(string filePath)
    {
        var data = C2ArrayParser.Parse(filePath);
        var records = new List<SectRecord>(data.Count);

        foreach (List<List<C2Value>> row in data)
            records.Add(new SectRecord { Groups = row });

        return records;
    }

    public static void Save(string filePath, List<SectRecord> records)
    {
        C2ArrayParser.Save(filePath, records.Select(r => (IReadOnlyList<IReadOnlyList<C2Value>>)r.Groups).ToList());
    }
}
