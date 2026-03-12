using XiuzhenSaveEditor.Models;

namespace XiuzhenSaveEditor.Parsers;

/// <summary>
/// Parses and serializes the svjy.dat (sect disciples) file.
///
/// The file is stored as a Construct c2array with one disciple per outer row.
/// </summary>
public static class DiscipleParser
{
    public static List<Disciple> Parse(string filePath)
    {
        var data = C2ArrayParser.Parse(filePath);
        var disciples = new List<Disciple>(data.Count);

        foreach (List<List<C2Value>> row in data)
        {
            var disciple = new Disciple
            {
                RawValues = row.Select(cell => cell.Count > 0 ? cell[0] : C2Value.CreateNumber()).ToList()
            };
            disciples.Add(disciple);
        }

        return disciples;
    }

    public static void Save(string filePath, List<Disciple> disciples)
    {
        C2ArrayParser.Save(filePath, disciples.Select(d => d.RawValues.Select(v => new[] { v })));
    }
}
