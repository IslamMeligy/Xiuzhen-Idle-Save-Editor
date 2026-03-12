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

            // Skill talent fields (positions 21-28)
            Assert.Equal("1", disciples[0].BuildTalent);
            Assert.Equal("2", disciples[0].HerbsTalent);
            Assert.Equal("3", disciples[0].MineTalent);
            Assert.Equal("4", disciples[0].HuntTalent);
            Assert.Equal("5", disciples[0].TameTalent);
            Assert.Equal("6", disciples[0].ExternalTalent);
            Assert.Equal("7", disciples[0].DanTalent);
            Assert.Equal("8", disciples[0].WeaponTalent);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void SkillTalentGrades_CorrectlyClassified()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            // Craft a line with known talent values at positions 21-28
            File.WriteAllText(tempFile,
                "[1],[Wang],[Wei],[uuid001],[85],[72],[63],[90],[45],[3],[5],[7],[4],[6],[8],[9],[10],[2],[1],[0],[0],[15],[35],[55],[85],[120],[145],[10],[100]");
            var disciples = DiscipleParser.Parse(tempFile);
            Assert.Single(disciples);
            Assert.Equal("C", disciples[0].BuildTalentGradeStr);     // 15 -> C
            Assert.Equal("B", disciples[0].HerbsTalentGradeStr);     // 35 -> B
            Assert.Equal("A", disciples[0].MineTalentGradeStr);      // 55 -> A
            Assert.Equal("S", disciples[0].HuntTalentGradeStr);      // 85 -> S
            Assert.Equal("SS", disciples[0].TameTalentGradeStr);     // 120 -> SS
            Assert.Equal("SSS", disciples[0].ExternalTalentGradeStr);// 145 -> SSS
            Assert.Equal("C", disciples[0].DanTalentGradeStr);       // 10 -> C
            Assert.Equal("S", disciples[0].WeaponTalentGradeStr);    // 100 -> S
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

public class ItemDatabaseTests
{
    [Fact]
    public void LoadEmbedded_ReturnsNonEmptyDatabase()
    {
        var db = ItemDatabase.LoadEmbedded();
        Assert.True(db.All.Count > 0);
    }

    [Fact]
    public void GetById_KnownItem_ReturnsCorrectRecord()
    {
        var db = ItemDatabase.LoadEmbedded();
        var item = db.GetById(1);
        Assert.NotNull(item);
        Assert.Equal("Sun Grass", item.Name);
        Assert.Equal("Gathering", item.Category);
        Assert.Equal(1, item.Tier);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        var db = ItemDatabase.LoadEmbedded();
        Assert.Null(db.GetById(99999));
    }

    [Fact]
    public void GetName_KnownId_ReturnsName()
    {
        var db = ItemDatabase.LoadEmbedded();
        Assert.Equal("Basalt Ore", db.GetName(16));
    }

    [Fact]
    public void GetName_UnknownId_ReturnsFallback()
    {
        var db = ItemDatabase.LoadEmbedded();
        Assert.Equal("Unknown (99999)", db.GetName(99999));
    }

    [Fact]
    public void ByCategory_ReturnsMatchingItems()
    {
        var db = ItemDatabase.LoadEmbedded();
        var gathering = db.ByCategory("Gathering").ToList();
        Assert.True(gathering.Count > 0);
        Assert.All(gathering, i => Assert.Equal("Gathering", i.Category));
    }

    [Fact]
    public void GetById_SoulItem_ReturnsCorrectRecord()
    {
        var db = ItemDatabase.LoadEmbedded();
        var soul = db.GetById(111);
        Assert.NotNull(soul);
        Assert.Equal("Wild Boar Beast Soul", soul.Name);
        Assert.Equal("Souls", soul.Category);
    }

    [Fact]
    public void GetById_BreakthroughMaterial_ReturnsCorrectRecord()
    {
        var db = ItemDatabase.LoadEmbedded();
        var item = db.GetById(481);
        Assert.NotNull(item);
        Assert.Equal("Chakra Herb", item.Name);
        Assert.Equal("Breakthrough Material", item.Category);
    }
}

public class InventoryParserTests
{
    [Fact]
    public void Parse_ValidLine_ReturnsInventoryRecord()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "[42,100]");
            var items = InventoryParser.Parse(tempFile);
            Assert.Single(items);
            Assert.Equal("42", items[0].ItemId);
            Assert.Equal("100", items[0].Quantity);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Parse_MultipleLines_ReturnsAllRecords()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "[1,50]\n[16,25]\n[91,10]");
            var items = InventoryParser.Parse(tempFile);
            Assert.Equal(3, items.Count);
            Assert.Equal("1",  items[0].ItemId);
            Assert.Equal("16", items[1].ItemId);
            Assert.Equal("91", items[2].ItemId);
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
            var items = InventoryParser.Parse(tempFile);
            Assert.Empty(items);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void InventoryRecord_ToFileLine_RoundTrip()
    {
        var inv = new InventoryRecord();
        inv.Groups.Add(new List<string> { "42", "100" });
        Assert.Equal("[42,100]", inv.ToFileLine());
    }

    [Fact]
    public void Save_RoundTrip_PreservesValues()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "[42,100]");
            var items = InventoryParser.Parse(tempFile);
            items[0].SetSub(0, 1, "200");
            InventoryParser.Save(tempFile, items);

            var reloaded = InventoryParser.Parse(tempFile);
            Assert.Equal("200", reloaded[0].Quantity);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
