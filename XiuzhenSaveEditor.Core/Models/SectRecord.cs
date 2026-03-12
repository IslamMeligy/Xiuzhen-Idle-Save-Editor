namespace XiuzhenSaveEditor.Models;

/// <summary>
/// Represents a raw sect-data row from the svmp.dat file.
/// The file is not fully mapped yet, so the editor exposes each group/subvalue pair directly.
/// </summary>
public class SectRecord
{
    public List<List<C2Value>> Groups { get; set; } = new();

    public bool IsEmpty =>
        Groups.All(group => group.All(value => string.IsNullOrWhiteSpace(value.Text) || value.Text == "0"));

    public string GetSub(int group, int sub)
    {
        if (group < Groups.Count && sub < Groups[group].Count)
            return Groups[group][sub].Text;

        return string.Empty;
    }

    public void SetSub(int group, int sub, string value)
    {
        while (Groups.Count <= group)
            Groups.Add(new List<C2Value>());

        while (Groups[group].Count <= sub)
            Groups[group].Add(CreateDefaultValue(sub));

        Groups[group][sub].SetText(value);
    }

    private static C2Value CreateDefaultValue(int sub) => sub switch
    {
        3 or 4 => C2Value.CreateString(),
        _ => C2Value.CreateNumber()
    };
}
