using XiuzhenSaveEditor.Models;

namespace XiuzhenSaveEditor.Parsers;

/// <summary>
/// Parses and serializes the save.dat file.
///
/// Each data line is formatted as: [ID],[val1],[val2],...
/// Key positions (0-indexed):
///   0  = ID
///   1  = Layer (for cultivation IDs 1-9)
///   4  = Base attribute
///   17 = Special value (pet stats for IDs 3-6)
///   22 = Main value (player talents, money, resources)
///   24 = Floor (mystic realm max floor, for IDs 1-9)
/// </summary>
public static class SaveDataParser
{
    /// <summary>Known cultivation technique IDs (1-9).</summary>
    public static readonly int[] CultivationIds = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

    /// <summary>Human-readable names for cultivation technique IDs.</summary>
    public static readonly Dictionary<int, string> CultivationNames = new()
    {
        { 1, "Breath" },
        { 2, "Technique 2" },
        { 3, "Technique 3" },
        { 4, "Technique 4" },
        { 5, "Technique 5" },
        { 6, "Technique 6" },
        { 7, "Technique 7" },
        { 8, "Technique 8" },
        { 9, "Technique 9" }
    };

    /// <summary>Mystic realm names indexed by cultivation ID.</summary>
    public static readonly Dictionary<int, string> MysticNames = new()
    {
        { 1, "Moon Peak" },
        { 2, "Cang Wolf Valley" },
        { 3, "Heavenly Pond" },
        { 4, "Mystic 4" },
        { 5, "Mystic 5" },
        { 6, "Mystic 6" },
        { 7, "Mystic 7" },
        { 8, "Mystic 8" },
        { 9, "Mystic 9" }
    };

    public static List<SaveRecord> Parse(string filePath)
    {
        var records = new List<SaveRecord>();
        if (!File.Exists(filePath)) return records;

        foreach (string rawLine in File.ReadAllLines(filePath))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var groups = BracketParser.ExtractGroups(line);
            if (groups.Count == 0) continue;

            if (!int.TryParse(groups[0], out int id)) continue;

            var record = new SaveRecord
            {
                Id = id,
                Values = groups
            };
            records.Add(record);
        }

        return records;
    }

    public static void Save(string filePath, List<SaveRecord> records)
    {
        var lines = records.Select(r => r.ToFileLine()).ToList();
        File.WriteAllLines(filePath, lines);
    }

    /// <summary>
    /// Returns the SaveRecord with the given ID, or null if not found.
    /// </summary>
    public static SaveRecord? FindById(List<SaveRecord> records, int id) =>
        records.FirstOrDefault(r => r.Id == id);
}
