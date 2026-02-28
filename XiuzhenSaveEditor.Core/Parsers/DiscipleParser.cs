using XiuzhenSaveEditor.Models;

namespace XiuzhenSaveEditor.Parsers;

/// <summary>
/// Parses and serializes the svjy.dat (sect disciples) file.
///
/// Column order (0-indexed):
///   0=Realm, 1=FamilyName, 2=Name, 3=UUID, 4=Qi-Sense, 5=God, 6=Roots,
///   7=Talent, 8=Chance, 9=Building, 10=Herbs, 11=Mining, 12=Hunting,
///   13=Taming, 14=External, 15=Alchemy, 16=Weapon, 17=Position, 18=Task,
///   19=?, 20=?, 21=Build, 22=Herbs, 23=Mine, 24=Hunt, 25=Tame,
///   26=External, 27=Dan, 28=Weapon
/// </summary>
public static class DiscipleParser
{
    public static List<Disciple> Parse(string filePath)
    {
        var disciples = new List<Disciple>();
        if (!File.Exists(filePath)) return disciples;

        foreach (string rawLine in File.ReadAllLines(filePath))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var groups = BracketParser.ExtractGroups(line);
            if (groups.Count == 0) continue;

            var disciple = new Disciple
            {
                RawValues = groups.Select(BracketParser.StripQuotes).ToList()
            };
            disciples.Add(disciple);
        }

        return disciples;
    }

    public static void Save(string filePath, List<Disciple> disciples)
    {
        var lines = disciples.Select(d =>
        {
            // Re-quote string fields (name fields at known positions)
            var values = d.RawValues.ToList();
            QuoteIfString(values, 1);  // FamilyName
            QuoteIfString(values, 2);  // Name
            return "[" + string.Join("],[", values) + "]";
        });
        File.WriteAllLines(filePath, lines);
    }

    private static void QuoteIfString(List<string> values, int index)
    {
        if (index >= values.Count) return;
        string v = values[index];
        if (v.Length == 0) return;
        if (!double.TryParse(v, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out _)
            && !v.StartsWith('"'))
        {
            values[index] = $"\"{v}\"";
        }
    }
}
