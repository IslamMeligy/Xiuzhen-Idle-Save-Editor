using XiuzhenSaveEditor.Models;

namespace XiuzhenSaveEditor.Parsers;

/// <summary>
/// Parses and serializes the svwp.dat (inventory) file.
///
/// Each line contains bracket groups with comma-separated sub-values:
///   [ItemID,Quantity]
/// </summary>
public static class InventoryParser
{
    public static List<InventoryRecord> Parse(string filePath)
    {
        var items = new List<InventoryRecord>();
        if (!File.Exists(filePath)) return items;

        foreach (string rawLine in File.ReadAllLines(filePath))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var groups = BracketParser.ExtractGroups(line);
            if (groups.Count == 0) continue;

            var record = new InventoryRecord();
            foreach (string g in groups)
            {
                record.Groups.Add(BracketParser.SplitGroup(g));
            }
            items.Add(record);
        }

        return items;
    }

    public static void Save(string filePath, List<InventoryRecord> items)
    {
        var lines = items.Select(i => i.ToFileLine()).ToList();
        File.WriteAllLines(filePath, lines);
    }
}
