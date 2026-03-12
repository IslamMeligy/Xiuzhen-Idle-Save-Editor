using System.Text.Json;
using XiuzhenSaveEditor.Models;

namespace XiuzhenSaveEditor.Parsers;

/// <summary>
/// Reads and writes Construct-style c2array JSON payloads.
/// </summary>
internal static class C2ArrayParser
{
    public static List<List<List<C2Value>>> Parse(string filePath)
    {
        var rows = new List<List<List<C2Value>>>();
        if (!File.Exists(filePath))
            return rows;

        using var document = JsonDocument.Parse(File.ReadAllText(filePath));
        var root = document.RootElement;

        if (!root.TryGetProperty("c2array", out JsonElement c2ArrayElement) || !c2ArrayElement.GetBoolean())
            throw new InvalidDataException($"File '{filePath}' is not a c2array payload.");

        if (!root.TryGetProperty("data", out JsonElement dataElement) || dataElement.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException($"File '{filePath}' is missing a valid c2array data section.");

        foreach (JsonElement rowElement in dataElement.EnumerateArray())
        {
            if (rowElement.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException($"File '{filePath}' contains an invalid c2array row.");

            var row = new List<List<C2Value>>();
            foreach (JsonElement cellElement in rowElement.EnumerateArray())
            {
                if (cellElement.ValueKind != JsonValueKind.Array)
                    throw new InvalidDataException($"File '{filePath}' contains an invalid c2array cell.");

                var cell = new List<C2Value>();
                foreach (JsonElement valueElement in cellElement.EnumerateArray())
                    cell.Add(C2Value.FromJsonElement(valueElement));

                row.Add(cell);
            }

            rows.Add(row);
        }

        return rows;
    }

    public static void Save(string filePath, IEnumerable<IEnumerable<IEnumerable<C2Value>>> rows)
    {
        var materializedRows = rows
            .Select(row => row.Select(cell => cell.ToList()).ToList())
            .ToList();

        int maxColumns = materializedRows.Count == 0 ? 0 : materializedRows.Max(r => r.Count);
        int maxDepth = materializedRows.Count == 0
            ? 0
            : materializedRows.Where(r => r.Count > 0).SelectMany(r => r).DefaultIfEmpty().Max(c => c?.Count ?? 0);

        using var stream = File.Create(filePath);
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false });

        writer.WriteStartObject();
        writer.WriteBoolean("c2array", true);

        writer.WritePropertyName("size");
        writer.WriteStartArray();
        writer.WriteNumberValue(materializedRows.Count);
        writer.WriteNumberValue(maxColumns);
        writer.WriteNumberValue(maxDepth);
        writer.WriteEndArray();

        writer.WritePropertyName("data");
        writer.WriteStartArray();
        foreach (List<List<C2Value>> row in materializedRows)
        {
            writer.WriteStartArray();
            foreach (List<C2Value> cell in row)
            {
                writer.WriteStartArray();
                foreach (C2Value value in cell)
                    value.WriteTo(writer);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}
