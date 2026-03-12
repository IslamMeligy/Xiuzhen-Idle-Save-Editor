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
/// Additional fields beyond the mapped range are preserved as-is.
/// </summary>
public class Disciple
{
    // Raw parsed values - index matches column position
    public List<C2Value> RawValues { get; set; } = new();

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

    // Columns 19-20: unknown purpose
    public string Unknown1   => Get(19);
    public string Unknown2   => Get(20);

    // Columns 21-28: skill talent values (graded C/B/A/S/SS/SSS)
    public string BuildTalent    => Get(21);
    public string HerbsTalent    => Get(22);
    public string MineTalent     => Get(23);
    public string HuntTalent     => Get(24);
    public string TameTalent     => Get(25);
    public string ExternalTalent => Get(26);
    public string DanTalent      => Get(27);
    public string WeaponTalent   => Get(28);

    // Talent grades for personal attributes (displayed as C/B/A/S/SS/SSS)
    public string TalentGradeStr  => TalentGrade.GetGrade(Talent);
    public string ChanceGradeStr  => TalentGrade.GetGrade(Chance);
    public string QiSenseGradeStr => TalentGrade.GetGrade(QiSense);
    public string GodGradeStr     => TalentGrade.GetGrade(God);
    public string RootsGradeStr   => TalentGrade.GetGrade(Roots);

    // Talent grades for skill fields (positions 21-28)
    public string BuildTalentGradeStr    => TalentGrade.GetGrade(BuildTalent);
    public string HerbsTalentGradeStr    => TalentGrade.GetGrade(HerbsTalent);
    public string MineTalentGradeStr     => TalentGrade.GetGrade(MineTalent);
    public string HuntTalentGradeStr     => TalentGrade.GetGrade(HuntTalent);
    public string TameTalentGradeStr     => TalentGrade.GetGrade(TameTalent);
    public string ExternalTalentGradeStr => TalentGrade.GetGrade(ExternalTalent);
    public string DanTalentGradeStr      => TalentGrade.GetGrade(DanTalent);
    public string WeaponTalentGradeStr   => TalentGrade.GetGrade(WeaponTalent);

    public bool IsEmpty =>
        RawValues.All(v => string.IsNullOrWhiteSpace(v.Text) || v.Text == "0");

    public string GetRaw(int index) => Get(index);

    private string Get(int i) => i < RawValues.Count ? RawValues[i].Text : "";

    public void Set(int index, string value)
    {
        while (RawValues.Count <= index)
            RawValues.Add(CreateDefaultValue(index));

        RawValues[index].SetText(value);
    }

    public string ToFileLine() =>
        "[" + string.Join("],[", RawValues.Select(v => v.Text)) + "]";

    private static C2Value CreateDefaultValue(int index) => index switch
    {
        1 or 2 or 17 or 18 => C2Value.CreateString(),
        _ => C2Value.CreateNumber()
    };
}
