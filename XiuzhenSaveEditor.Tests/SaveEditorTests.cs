using System.Text.Json;
using XiuzhenSaveEditor.Models;
using XiuzhenSaveEditor.Parsers;

namespace XiuzhenSaveEditor.Tests;

internal static class TestData
{
    public static object[] Cell(params object[] values) => values;

    public static object[][] Row(params object[][] cells) => cells;

    public static C2Value Num(string text = "0") => C2Value.CreateNumber(text);

    public static C2Value Str(string text = "") => C2Value.CreateString(text);

    public static void WriteC2Array(string path, params object[][][] rows)
    {
        int maxCols = rows.Length == 0 ? 0 : rows.Max(row => row.Length);
        int maxDepth = 0;
        foreach (object[][] row in rows)
        {
            foreach (object[] cell in row)
                maxDepth = Math.Max(maxDepth, cell.Length);
        }

        var payload = new
        {
            c2array = true,
            size = new[] { rows.Length, maxCols, maxDepth },
            data = rows
        };

        File.WriteAllText(path, JsonSerializer.Serialize(payload));
    }

    public static string ExampleFile(string fileName) =>
        Path.Combine(RepoRoot(), "XiuzhenSaveEditor.Core", "Example", fileName);

    private static string RepoRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "XiuzhenSaveEditor.Core")))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root from test output directory.");
    }
}

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
}

public class SaveDataParserTests
{
    [Fact]
    public void Parse_ValidC2Array_ReturnsRecordWithCorrectId()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            TestData.WriteC2Array(
                tempFile,
                TestData.Row(
                    TestData.Cell(1),
                    TestData.Cell(1050),
                    TestData.Cell(0),
                    TestData.Cell(1),
                    TestData.Cell(9),
                    TestData.Cell(0),
                    TestData.Cell(0),
                    TestData.Cell(0),
                    TestData.Cell(0),
                    TestData.Cell(0),
                    TestData.Cell(0),
                    TestData.Cell(1579),
                    TestData.Cell(1091),
                    TestData.Cell(1),
                    TestData.Cell(0),
                    TestData.Cell(1),
                    TestData.Cell(0),
                    TestData.Cell(40),
                    TestData.Cell(1),
                    TestData.Cell(0),
                    TestData.Cell(0),
                    TestData.Cell(1),
                    TestData.Cell(120),
                    TestData.Cell(0),
                    TestData.Cell(500)
                ),
                TestData.Row(
                    TestData.Cell(17),
                    TestData.Cell(0),
                    TestData.Cell(0),
                    TestData.Cell(1),
                    TestData.Cell(2),
                    TestData.Cell(0),
                    TestData.Cell(0),
                    TestData.Cell(0),
                    TestData.Cell(0),
                    TestData.Cell(0),
                    TestData.Cell(0),
                    TestData.Cell(0),
                    TestData.Cell(0),
                    TestData.Cell(1),
                    TestData.Cell(0),
                    TestData.Cell(1),
                    TestData.Cell(0),
                    TestData.Cell(0),
                    TestData.Cell(1),
                    TestData.Cell(0),
                    TestData.Cell(0),
                    TestData.Cell(1),
                    TestData.Cell(999999),
                    TestData.Cell(0),
                    TestData.Cell(0)
                )
            );

            var records = SaveDataParser.Parse(tempFile);
            Assert.Equal(2, records.Count);
            Assert.Equal(1, records[0].Id);
            Assert.Equal("1050", records[0].GetValue(1));
            Assert.Equal("500", records[0].GetValue(24));
            Assert.Equal(17, records[1].Id);
            Assert.Equal("999999", records[1].GetValue(22));
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
            new()
            {
                Id = 5,
                Cells =
                [
                    [TestData.Num("5")],
                    [TestData.Num("100")],
                    [TestData.Num("0")]
                ]
            }
        };

        var found = SaveDataParser.FindById(records, 5);
        Assert.NotNull(found);
        Assert.Equal(5, found!.Id);
    }

    [Fact]
    public void Save_RoundTrip_PreservesValues()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            TestData.WriteC2Array(
                tempFile,
                TestData.Row(TestData.Cell(1), TestData.Cell(1050), TestData.Cell(0), TestData.Cell(1))
            );

            var records = SaveDataParser.Parse(tempFile);
            Assert.Single(records);

            records[0].SetValue(1, "2000");
            SaveDataParser.Save(tempFile, records);

            var reloaded = SaveDataParser.Parse(tempFile);
            Assert.Equal("2000", reloaded[0].GetValue(1));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Parse_ExampleFile_ReturnsExpectedKnownValues()
    {
        var records = SaveDataParser.Parse(TestData.ExampleFile("0save.dat"));

        Assert.Contains(records, record => record.Id == 1 && record.GetValue(1) == "3100" && record.GetValue(24) == "251");
        Assert.Contains(records, record => record.Id == 17 && record.GetValue(22) == "7018659007328726");
    }
}

public class DiscipleParserTests
{
    [Fact]
    public void Parse_ValidC2Array_ReturnsDisciple()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            TestData.WriteC2Array(
                tempFile,
                TestData.Row(
                    TestData.Cell(1),
                    TestData.Cell("Wang"),
                    TestData.Cell("Wei"),
                    TestData.Cell(12345),
                    TestData.Cell(85),
                    TestData.Cell(72),
                    TestData.Cell(63),
                    TestData.Cell(90),
                    TestData.Cell(45),
                    TestData.Cell(3),
                    TestData.Cell(5),
                    TestData.Cell(7),
                    TestData.Cell(4),
                    TestData.Cell(6),
                    TestData.Cell(8),
                    TestData.Cell(9),
                    TestData.Cell(10),
                    TestData.Cell("No position"),
                    TestData.Cell("Forging!"),
                    TestData.Cell(11),
                    TestData.Cell(12),
                    TestData.Cell(1),
                    TestData.Cell(2),
                    TestData.Cell(3),
                    TestData.Cell(4),
                    TestData.Cell(5),
                    TestData.Cell(6),
                    TestData.Cell(7),
                    TestData.Cell(8)
                )
            );

            var disciples = DiscipleParser.Parse(tempFile);
            Assert.Single(disciples);
            Assert.Equal("Wang", disciples[0].FamilyName);
            Assert.Equal("Wei", disciples[0].Name);
            Assert.Equal("No position", disciples[0].Position);
            Assert.Equal("Forging!", disciples[0].Task);
            Assert.Equal("11", disciples[0].Unknown1);
            Assert.Equal("2", disciples[0].HerbsTalent);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Parse_ExampleFile_ExposesHiddenColumns()
    {
        var disciples = DiscipleParser.Parse(TestData.ExampleFile("0svjy.dat"));
        var disciple = disciples[1];

        Assert.Equal("Li", disciple.FamilyName);
        Assert.Equal("Xiao Yao", disciple.Name);
        Assert.Equal("No position", disciple.Position);
        Assert.Equal("Forging!", disciple.Task);
        Assert.Equal("214", disciple.Unknown1);
        Assert.Equal("12345", disciple.BuildTalent);
        Assert.Equal("0", disciple.GetRaw(29));
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
}

public class SoulParserTests
{
    [Fact]
    public void Parse_ValidC2Array_ReturnsSoulRecord()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            TestData.WriteC2Array(
                tempFile,
                TestData.Row(
                    TestData.Cell(100, 0, ""),
                    TestData.Cell(5, 0, 42),
                    TestData.Cell(34, 0, 36),
                    TestData.Cell(58, 0, 240),
                    TestData.Cell(720, 0, 3),
                    TestData.Cell(82, 0, "Mature"),
                    TestData.Cell(15, 0, "Dragon Heart"),
                    TestData.Cell(0, 0, "Mature1")
                )
            );

            var souls = SoulParser.Parse(tempFile);
            Assert.Single(souls);
            Assert.Equal("100", souls[0].SoulId);
            Assert.Equal("42", souls[0].FruitId);
            Assert.Equal("34", souls[0].Spirit);
            Assert.Equal("Dragon Heart", souls[0].HarvestName);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Parse_ExampleFile_ReturnsExpectedFields()
    {
        var souls = SoulParser.Parse(TestData.ExampleFile("0svlz.dat"));
        var soul = souls[1];

        Assert.Equal("190", soul.SoulId);
        Assert.Equal("6", soul.FruitId);
        Assert.Equal("59", soul.Spirit);
        Assert.Equal("Aura fruit", soul.HarvestName);
    }

    [Fact]
    public void SoulRecord_ToFileLine_RoundTrip()
    {
        var soul = new SoulRecord
        {
            Groups =
            [
                [TestData.Num("100"), TestData.Num("0"), TestData.Str()],
                [TestData.Num("5"), TestData.Num("0"), TestData.Num("42")]
            ]
        };

        string line = soul.ToFileLine();
        Assert.Equal("[100,0,],[5,0,42]", line);
    }
}

public class LifestoneParserTests
{
    [Fact]
    public void Parse_ValidC2Array_ReturnsLifestoneRecord()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            TestData.WriteC2Array(
                tempFile,
                TestData.Row(
                    TestData.Cell(59, 1001),
                    TestData.Cell(110.15, 5),
                    TestData.Cell(0, 12345),
                    TestData.Cell(0, 12),
                    TestData.Cell(0, 7),
                    TestData.Cell(0, 3),
                    TestData.Cell(0, 9),
                    TestData.Cell(0, 0),
                    TestData.Cell(0, 15),
                    TestData.Cell(0, 85),
                    TestData.Cell(0, 72),
                    TestData.Cell(0, 60),
                    TestData.Cell(0, 55),
                    TestData.Cell(0, 90)
                )
            );

            var stones = LifestoneParser.Parse(tempFile);
            Assert.Single(stones);
            Assert.Equal("1001", stones[0].LifestoneId);
            Assert.Equal("5", stones[0].Level);
            Assert.Equal("12", stones[0].Effect1Id);
            Assert.Equal("85", stones[0].Effect1Pct);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Parse_ExampleFile_ReturnsExpectedLevel()
    {
        var stones = LifestoneParser.Parse(TestData.ExampleFile("0svzb.dat"));
        var stone = stones[1];

        Assert.Equal("471", stone.LifestoneId);
        Assert.Equal("1", stone.Level);
        Assert.Equal("12.3", stone.Effect1Pct);
    }
}

public class SaveRecordTests
{
    [Fact]
    public void GetValue_OutOfRange_ReturnsZero()
    {
        var rec = new SaveRecord
        {
            Id = 1,
            Cells =
            [
                [TestData.Num("1")],
                [TestData.Num("100")]
            ]
        };

        Assert.Equal("0", rec.GetValue(99));
    }

    [Fact]
    public void SetValue_ExtendsValues_WhenIndexBeyondCount()
    {
        var rec = new SaveRecord
        {
            Id = 1,
            Cells =
            [
                [TestData.Num("1")]
            ]
        };

        rec.SetValue(3, "42");
        Assert.Equal("42", rec.GetValue(3));
        Assert.Equal(4, rec.Cells.Count);
    }

    [Fact]
    public void ToFileLine_ProducesCorrectFormat()
    {
        var rec = new SaveRecord
        {
            Id = 1,
            Cells =
            [
                [TestData.Num("1")],
                [TestData.Num("1050")],
                [TestData.Num("0")]
            ]
        };

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
        Assert.Equal("Sun Grass", item!.Name);
        Assert.Equal("Gathering", item.Category);
        Assert.Equal(1, item.Tier);
    }

    [Fact]
    public void GetName_UnknownId_ReturnsFallback()
    {
        var db = ItemDatabase.LoadEmbedded();
        Assert.Equal("Unknown (99999)", db.GetName(99999));
    }
}

public class InventoryParserTests
{
    [Fact]
    public void Parse_ValidC2Array_ReturnsInventoryRecord()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            TestData.WriteC2Array(tempFile, TestData.Row(TestData.Cell(42), TestData.Cell(100), TestData.Cell(0)));

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
    public void Save_RoundTrip_PreservesValues()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            TestData.WriteC2Array(tempFile, TestData.Row(TestData.Cell(42), TestData.Cell(100), TestData.Cell(0)));
            var items = InventoryParser.Parse(tempFile);
            items[0].SetSub(1, 0, "200");
            InventoryParser.Save(tempFile, items);

            var reloaded = InventoryParser.Parse(tempFile);
            Assert.Equal("200", reloaded[0].Quantity);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Parse_ExampleFile_ReturnsExpectedQuantity()
    {
        var items = InventoryParser.Parse(TestData.ExampleFile("0svwp.dat"));
        var item = items[1];

        Assert.Equal("2", item.ItemId);
        Assert.Equal("3.9176267217271093E+186", double.Parse(item.Quantity).ToString("E16"));
    }
}

public class SectParserTests
{
    [Fact]
    public void Parse_ExampleFile_ReturnsExpectedRows()
    {
        var sects = SectParser.Parse(TestData.ExampleFile("0svmp.dat"));

        Assert.Equal(151, sects.Count);
        Assert.Equal("50000", sects[0].GetSub(7, 0));
        Assert.Equal("489", sects[1].GetSub(0, 1));
        Assert.Equal("67.59", sects[1].GetSub(3, 5));
    }

    [Fact]
    public void Save_RoundTrip_PreservesValues()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            SectParser.Save(
                tempFile,
                new List<SectRecord>
                {
                    new()
                    {
                        Groups =
                        [
                            [TestData.Num("1"), TestData.Num("2")],
                            [TestData.Num("3"), TestData.Str("note")]
                        ]
                    }
                }
            );

            var sects = SectParser.Parse(tempFile);
            Assert.Single(sects);
            Assert.Equal("2", sects[0].GetSub(0, 1));
            Assert.Equal("note", sects[0].GetSub(1, 1));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
