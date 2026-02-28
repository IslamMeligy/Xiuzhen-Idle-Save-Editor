namespace XiuzhenSaveEditor.Models;

/// <summary>
/// Represents a single data row in the save.dat file.
/// Each row starts with an ID and contains positional values.
/// </summary>
public class SaveRecord
{
    public int Id { get; set; }
    public List<string> Values { get; set; } = new();

    public string GetValue(int index) =>
        index < Values.Count ? Values[index] : "0";

    public void SetValue(int index, string value)
    {
        while (Values.Count <= index)
            Values.Add("0");
        Values[index] = value;
    }

    /// <summary>
    /// Serialize back to the bracket-delimited format.
    /// </summary>
    public string ToFileLine() =>
        "[" + string.Join("],[", Values) + "]";
}
