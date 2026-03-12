using XiuzhenSaveEditor.Models;

namespace XiuzhenSaveEditor.Parsers;

/// <summary>
/// Parses and serializes the svwp.dat (inventory) file.
///
/// The file is stored as a Construct c2array with one inventory row per outer record.
/// </summary>
public static class InventoryParser
{
    public static List<InventoryRecord> Parse(string filePath)
    {
        var data = C2ArrayParser.Parse(filePath);
        var items = new List<InventoryRecord>(data.Count);

        foreach (List<List<C2Value>> row in data)
        {
            items.Add(new InventoryRecord { Groups = row });
        }

        return items;
    }

    public static void Save(string filePath, List<InventoryRecord> items)
    {
        C2ArrayParser.Save(filePath, items.Select(i => (IReadOnlyList<IReadOnlyList<C2Value>>)i.Groups).ToList());
    }
}
