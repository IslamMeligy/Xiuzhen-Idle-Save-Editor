using System.Globalization;
using XiuzhenSaveEditor.Models;

namespace XiuzhenSaveEditor.Parsers;

/// <summary>
/// Parses and serializes the save.dat file.
///
/// The file is stored as a Construct c2array. The editor currently uses the first
/// value inside each field cell to expose key positions such as layer, floor,
/// pet stats, resources, and experiences.
/// </summary>
public static class SaveDataParser
{
    public const int PosId = 0;
    public const int PosLayer = 1;
    public const int PosSpecial = 17;
    public const int PosMainValue = 22;
    public const int PosFloor = 24;

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
        var data = C2ArrayParser.Parse(filePath);
        var records = new List<SaveRecord>(data.Count);

        foreach (List<List<C2Value>> row in data)
        {
            var record = new SaveRecord
            {
                Cells = row
            };

            if (int.TryParse(record.GetValue(PosId), NumberStyles.Any, CultureInfo.InvariantCulture, out int id))
                record.Id = id;

            records.Add(record);
        }

        return records;
    }

    public static void Save(string filePath, List<SaveRecord> records)
    {
        C2ArrayParser.Save(filePath, records.Select(r => r.Cells));
    }

    /// <summary>
    /// Returns the SaveRecord with the given ID, or null if not found.
    /// </summary>
    public static SaveRecord? FindById(List<SaveRecord> records, int id) =>
        records.FirstOrDefault(r => r.Id == id);
}
