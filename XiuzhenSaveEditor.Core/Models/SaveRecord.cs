namespace XiuzhenSaveEditor.Models;

/// <summary>
/// Represents a single record in the save.dat file.
/// Each logical field is stored in a nested c2array cell, and the editor
/// currently uses the first value in each field.
/// </summary>
public class SaveRecord
{
    public int Id { get; set; }
    public List<List<C2Value>> Cells { get; set; } = new();

    public string GetValue(int index) =>
        index < Cells.Count && Cells[index].Count > 0 ? Cells[index][0].Text : "0";

    public void SetValue(int index, string value)
    {
        while (Cells.Count <= index)
            Cells.Add([C2Value.CreateNumber()]);

        if (Cells[index].Count == 0)
            Cells[index].Add(C2Value.CreateNumber());

        Cells[index][0].SetText(value);
    }

    /// <summary>
    /// Debug-friendly representation of the first value in each field.
    /// </summary>
    public string ToFileLine() =>
        "[" + string.Join("],[", Cells.Select(c => c.Count > 0 ? c[0].Text : "0")) + "]";
}
