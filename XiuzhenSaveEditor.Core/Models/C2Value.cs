using System.Text.Json;

namespace XiuzhenSaveEditor.Models;

/// <summary>
/// Represents a single primitive value inside a Construct c2array cell.
/// </summary>
public sealed class C2Value
{
    private C2Value(string text, JsonValueKind kind)
    {
        Text = text;
        Kind = kind;
    }

    public string Text { get; private set; }
    public JsonValueKind Kind { get; }

    public static C2Value FromJsonElement(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => new C2Value(element.GetString() ?? string.Empty, JsonValueKind.String),
        JsonValueKind.Number => new C2Value(element.GetRawText(), JsonValueKind.Number),
        JsonValueKind.True => new C2Value("true", JsonValueKind.True),
        JsonValueKind.False => new C2Value("false", JsonValueKind.False),
        JsonValueKind.Null => new C2Value(string.Empty, JsonValueKind.Null),
        _ => throw new InvalidDataException($"Unsupported c2array value kind: {element.ValueKind}")
    };

    public static C2Value CreateNumber(string text = "0") => new(text, JsonValueKind.Number);

    public static C2Value CreateString(string text = "") => new(text, JsonValueKind.String);

    public void SetText(string text) => Text = text;

    public void WriteTo(Utf8JsonWriter writer)
    {
        switch (Kind)
        {
            case JsonValueKind.String:
                writer.WriteStringValue(Text);
                break;
            case JsonValueKind.Number:
                writer.WriteRawValue(string.IsNullOrWhiteSpace(Text) ? "0" : Text.Trim(), skipInputValidation: false);
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                writer.WriteBooleanValue(string.Equals(Text, "true", StringComparison.OrdinalIgnoreCase));
                break;
            case JsonValueKind.Null:
                writer.WriteNullValue();
                break;
            default:
                throw new InvalidDataException($"Unsupported c2array value kind: {Kind}");
        }
    }
}
