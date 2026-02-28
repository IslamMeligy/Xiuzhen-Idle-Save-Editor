using XiuzhenSaveEditor.Parsers;
using XiuzhenSaveEditor.Models;

namespace XiuzhenSaveEditor.Tests;

public class BracketParserTests
{
    [Fact]
    public void ExtractGroups_SimpleValues_ReturnsCorrectGroups()
    {
        var groups = BracketParser.ExtractGroups("[1],[1050],[0],[9]");
        Assert.Equal(4, groups.Count);
        Assert.Equal("1", groups[0]);
        Assert.Equal("1050", groups[1]);
        Assert.Equal("0", groups[2]);
        Assert.Equal("9", groups[3]);
    }

    [Fact]
    public void ExtractGroups_QuotedString_IncludesQuotes()
    {
        var groups = BracketParser.ExtractGroups("[1],[\"Dragon Heart\"],[0]");
        Assert.Equal(3, groups.Count);
        Assert.Equal("\"Dragon Heart\"", groups[1]);
    }

    [Fact]
    public void ExtractGroups_EmptyLine_ReturnsEmpty()
    {
        var groups = BracketParser.ExtractGroups("");
        Assert.Empty(groups);
    }

    [Fact]
    public void SplitGroup_SimpleCSV_SplitsCorrectly()
    {
        var parts = BracketParser.SplitGroup("34,0,36");
        Assert.Equal(3, parts.Count);
        Assert.Equal("34", parts[0]);
        Assert.Equal("0", parts[1]);
        Assert.Equal("36", parts[2]);
    }

    [Fact]
    public void SplitGroup_WithQuotedComma_DoesNotSplitInsideQuotes()
    {
        var parts = BracketParser.SplitGroup("15,0,\"Dragon Heart\"");
        Assert.Equal(3, parts.Count);
        Assert.Equal("\"Dragon Heart\"", parts[2]);
    }

    [Fact]
    public void StripQuotes_QuotedValue_RemovesQuotes()
    {
        Assert.Equal("Dragon Heart", BracketParser.StripQuotes("\"Dragon Heart\""));
    }

    [Fact]
    public void StripQuotes_UnquotedValue_Unchanged()
    {
        Assert.Equal("42", BracketParser.StripQuotes("42"));
    }
}

public class SaveDataParserTests
{
    [Fact]
    public void Parse_ValidLine_ReturnsRecordWithCorrectId()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile,
                "[1],[1050],[0],[1],[9],[0],[0],[0],[0],[0],[0],[1579],[1091],[1],[0],[1],[0],[40],[1],[0],[0],[1],[120],[0],[500],[101],[0],[10],[0],[8]\n" +
                "[17],[0],[0],[1],[2],[0],[0],[0],[0],[0],[0],[0],[0],[1],[0],[1],[0],[0],[1],[0],[0],[1],[999999],[0],[0],[0],[0],[0],[0],[2]");

            var records = SaveDataParser.Parse(tempFile);
            Assert.Equal(2, records.Count);
            Assert.Equal(1, records[0].Id);
            Assert.Equal("1050", records[0].GetValue(1));  // Layer
            Assert.Equal("500", records[0].GetValue(24));  // Floor

            Assert.Equal(17, records[1].Id);
            Assert.Equal("999999", records[1].GetValue(22)); // Money
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Parse_EmptyFile_ReturnsEmptyList()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "");
            var records = SaveDataParser.Parse(tempFile);
            Assert.Empty(records);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void FindById_ExistingId_ReturnsRecord()
    {
        var records = new List<SaveRecord>
        {
            new() { Id = 5, Values = ["5", "100", "0"] }
        };
        var found = SaveDataParser.FindById(records, 5);
        Assert.NotNull(found);
        Assert.Equal(5, found.Id);
    }

    [Fact]
    public void FindById_MissingId_ReturnsNull()
    {
        var records = new List<SaveRecord>
        {
            new() { Id = 5, Values = ["5", "100", "0"] }
        };
        Assert.Null(SaveDataParser.FindById(records, 99));
    }

    [Fact]
    public void Save_RoundTrip_PreservesValues()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            string original =
                "[1],[1050],[0],[1],[9],[0],[0],[0],[0],[0],[0],[1579],[1091],[1],[0],[1],[0],[40],[1],[0],[0],[1],[120],[0],[500],[101],[0],[10],[0],[8]";
            File.WriteAllText(tempFile, original);

            var records = SaveDataParser.Parse(tempFile);
            Assert.Single(records);

            records[0].SetValue(1, "2000"); // change layer
            SaveDataParser.Save(tempFile, records);

            var reloaded = SaveDataParser.Parse(tempFile);
            Assert.Equal("2000", reloaded[0].GetValue(1));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}

public class DiscipleParserTests
{
    private const string SampleLine =
        "[1],[Wang],[Wei],[uuid001],[85],[72],[63],[90],[45],[3],[5],[7],[4],[6],[8],[9],[10],[2],[1],[0],[0],[1],[2],[3],[4],[5],[6],[7],[8]";

    [Fact]
    public void Parse_ValidLine_ReturnsDisciple()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, SampleLine);
            var disciples = DiscipleParser.Parse(tempFile);
            Assert.Single(disciples);
            Assert.Equal("1", disciples[0].Realm);
            Assert.Equal("Wang", disciples[0].FamilyName);
            Assert.Equal("Wei", disciples[0].Name);
            Assert.Equal("85", disciples[0].QiSense);
            Assert.Equal("90", disciples[0].Talent);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void TalentGrade_CorrectlyClassifies()
    {
        Assert.Equal("C", TalentGrade.GetGrade(15));
        Assert.Equal("B", TalentGrade.GetGrade(30));
        Assert.Equal("A", TalentGrade.GetGrade(55));
        Assert.Equal("S", TalentGrade.GetGrade(85));
        Assert.Equal("SS", TalentGrade.GetGrade(120));
        Assert.Equal("SSS", TalentGrade.GetGrade(145));
    }

    [Fact]
    public void TalentGrade_Zero_ReturnsDash()
    {
        Assert.Equal("-", TalentGrade.GetGrade(0));
    }
}

public class SoulParserTests
{
    [Fact]
    public void Parse_ValidLine_ReturnsSoulRecord()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            // [SID,0,0],[SQ,0,FID],[ST1,0,LSU],[ST2,0,LS],[ST3,0,FA],[ST4,0,0],[ST5,0,"HarvestName"],[0,0,0]
            File.WriteAllText(tempFile,
                "[100,0,0],[5,0,42],[34,0,36],[58,0,240],[720,0,3],[82,0,0],[15,0,\"Dragon Heart\"],[0,0,0]");

            var souls = SoulParser.Parse(tempFile);
            Assert.Single(souls);
            Assert.Equal("100", souls[0].SoulId);
            Assert.Equal("42",  souls[0].FruitId);
            Assert.Equal("34",  souls[0].Spirit);
            Assert.Equal("58",  souls[0].Resonance);
            Assert.Equal("\"Dragon Heart\"", souls[0].HarvestName);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void SoulRecord_ToFileLine_RoundTrip()
    {
        var soul = new SoulRecord();
        soul.Groups.Add(new List<string> { "100", "0", "0" });
        soul.Groups.Add(new List<string> { "5", "0", "42" });

        string line = soul.ToFileLine();
        Assert.Equal("[100,0,0],[5,0,42]", line);
    }
}

public class LifestoneParserTests
{
    [Fact]
    public void Parse_ValidLine_ReturnsLifestoneRecord()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile,
                "[0,1001],[0,0],[0,5],[0,12],[0,7],[0,3],[0,9],[0,0],[0,15],[0,85],[0,72],[0,60],[0,55],[0,90]");

            var stones = LifestoneParser.Parse(tempFile);
            Assert.Single(stones);
            Assert.Equal("1001", stones[0].LifestoneId);
            Assert.Equal("5",    stones[0].Level);
            Assert.Equal("12",   stones[0].Effect1Id);
            Assert.Equal("85",   stones[0].Effect1Pct);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void LifestoneRecord_ToFileLine_RoundTrip()
    {
        var ls = new LifestoneRecord();
        ls.Groups.Add(new List<string> { "0", "1001" });
        ls.Groups.Add(new List<string> { "0", "0" });
        ls.Groups.Add(new List<string> { "0", "5" });

        string line = ls.ToFileLine();
        Assert.Equal("[0,1001],[0,0],[0,5]", line);
    }
}

public class SaveRecordTests
{
    [Fact]
    public void GetValue_OutOfRange_ReturnsZero()
    {
        var rec = new SaveRecord { Id = 1, Values = new List<string> { "1", "100" } };
        Assert.Equal("0", rec.GetValue(99));
    }

    [Fact]
    public void SetValue_ExtendsValues_WhenIndexBeyondCount()
    {
        var rec = new SaveRecord { Id = 1, Values = new List<string> { "1" } };
        rec.SetValue(3, "42");
        Assert.Equal("42", rec.GetValue(3));
        Assert.Equal(4, rec.Values.Count);
    }

    [Fact]
    public void ToFileLine_ProducesCorrectFormat()
    {
        var rec = new SaveRecord { Id = 1, Values = new List<string> { "1", "1050", "0" } };
        Assert.Equal("[1],[1050],[0]", rec.ToFileLine());
    }
}
