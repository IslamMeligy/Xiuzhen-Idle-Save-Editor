using System.Globalization;
using System.Reflection;
using System.Text.Json;
using XiuzhenSaveEditor.Models;

namespace XiuzhenSaveEditor.Parsers;

/// <summary>
/// Loads Items.json (c2array format) from an embedded resource and provides
/// fast lookup by item ID.
/// </summary>
public class ItemDatabase
{
    private readonly Dictionary<int, ItemRecord> _items = new();

    private ItemDatabase() { }

    /// <summary>
    /// Creates a shared instance loaded from the embedded Items.json resource.
    /// </summary>
    public static ItemDatabase LoadEmbedded()
    {
        var asm = Assembly.GetAssembly(typeof(ItemDatabase))
            ?? throw new InvalidOperationException("Cannot locate Core assembly.");

        string resourceName = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("Items.json", StringComparison.OrdinalIgnoreCase))
            ?? throw new FileNotFoundException("Embedded resource Items.json not found.");

        using var stream = asm.GetManifestResourceStream(resourceName)!;
        return Load(stream);
    }

    /// <summary>
    /// Loads from any stream containing c2array JSON.
    /// </summary>
    public static ItemDatabase Load(Stream stream)
    {
        using var doc = JsonDocument.Parse(stream);
        var db = new ItemDatabase();

        var data = doc.RootElement.GetProperty("data");

        foreach (var row in data.EnumerateArray())
        {
            // Each row is an array of 9 columns; each column is [value].
            var item = new ItemRecord();

            item.Id        = GetInt(row, 0);
            item.Name      = GetString(row, 1);
            item.Category  = GetString(row, 2);
            item.Tier      = GetInt(row, 3);
            item.SubId     = GetInt(row, 4);
            item.StackFlag = GetInt(row, 5);
            item.Value     = GetDouble(row, 6);
            item.DisplayName = GetString(row, 7);
            item.Description = GetString(row, 8);

            if (item.Id != 0)
                db._items[item.Id] = item;
        }

        return db;
    }

    /// <summary>
    /// Returns the item with the given ID, or null if not found.
    /// </summary>
    public ItemRecord? GetById(int id) =>
        _items.TryGetValue(id, out var item) ? item : null;

    /// <summary>
    /// Returns the display name for the given item ID, or a fallback string.
    /// </summary>
    public string GetName(int id) =>
        GetById(id)?.Name ?? $"Unknown ({id})";

    /// <summary>
    /// Returns all loaded items.
    /// </summary>
    public IReadOnlyCollection<ItemRecord> All => _items.Values;

    /// <summary>
    /// Returns all items matching the given category.
    /// </summary>
    public IEnumerable<ItemRecord> ByCategory(string category) =>
        _items.Values.Where(i => i.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

    // ── c2array helpers ───────────────────────────────────────────────────

    private static string GetString(JsonElement row, int col)
    {
        var cell = row[col][0];
        return cell.ValueKind == JsonValueKind.String ? cell.GetString() ?? "" : cell.GetRawText();
    }

    private static int GetInt(JsonElement row, int col)
    {
        var cell = row[col][0];
        if (cell.ValueKind == JsonValueKind.Number)
            return cell.GetInt32();
        if (cell.ValueKind == JsonValueKind.String &&
            int.TryParse(cell.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out int v))
            return v;
        return 0;
    }

    private static double GetDouble(JsonElement row, int col)
    {
        var cell = row[col][0];
        if (cell.ValueKind == JsonValueKind.Number)
            return cell.GetDouble();
        if (cell.ValueKind == JsonValueKind.String &&
            double.TryParse(cell.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double v))
            return v;
        return 0;
    }
}
