namespace XiuzhenSaveEditor.Models;

/// <summary>
/// Talent grade thresholds used in svjy.dat.
/// C=1-20, B=21-40, A=41-70, S=71-100, SS=101-140, SSS=141-150
/// </summary>
public static class TalentGrade
{
    public static string GetGrade(double value) => value switch
    {
        <= 0 => "-",
        <= 20 => "C",
        <= 40 => "B",
        <= 70 => "A",
        <= 100 => "S",
        <= 140 => "SS",
        _ => "SSS"
    };

    public static string GetGrade(string raw) =>
        double.TryParse(raw, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out double v)
            ? GetGrade(v) : "-";
}

/// <summary>
/// Represents a sect disciple from the svjy.dat file.
/// Column order: [Realm],[Family Name],[Name],[UUID],[Qi-Sense],[God],[Roots],[Talent],
///               [Chance],[Building],[Herbs],[Mining],[Hunting],[Taming],[External],
///               [Alchemy],[Weapon],[Position],[Task],[?],[?],[Build],[Herbs],[Mine],
///               [Hunt],[Tame],[External],[Dan],[Weapon]
/// </summary>
public class Disciple
{
    // Raw parsed values - index matches column position
    public List<string> RawValues { get; set; } = new();

    public string Realm      => Get(0);
    public string FamilyName => Get(1);
    public string Name       => Get(2);
    public string UUID       => Get(3);
    public string QiSense    => Get(4);
    public string God        => Get(5);
    public string Roots      => Get(6);
    public string Talent     => Get(7);
    public string Chance     => Get(8);
    public string Building   => Get(9);
    public string Herbs      => Get(10);
    public string Mining     => Get(11);
    public string Hunting    => Get(12);
    public string Taming     => Get(13);
    public string External   => Get(14);
    public string Alchemy    => Get(15);
    public string Weapon     => Get(16);
    public string Position   => Get(17);
    public string Task       => Get(18);

    // Talent grades (displayed as C/B/A/S/SS/SSS)
    public string TalentGradeStr  => TalentGrade.GetGrade(Talent);
    public string ChanceGradeStr  => TalentGrade.GetGrade(Chance);
    public string QiSenseGradeStr => TalentGrade.GetGrade(QiSense);
    public string GodGradeStr     => TalentGrade.GetGrade(God);
    public string RootsGradeStr   => TalentGrade.GetGrade(Roots);

    private string Get(int i) => i < RawValues.Count ? RawValues[i] : "";

    public void Set(int index, string value)
    {
        while (RawValues.Count <= index)
            RawValues.Add("");
        RawValues[index] = value;
    }

    public string ToFileLine() =>
        "[" + string.Join("],[", RawValues) + "]";
}
