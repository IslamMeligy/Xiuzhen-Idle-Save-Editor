using XiuzhenSaveEditor.Models;
using XiuzhenSaveEditor.Parsers;

namespace XiuzhenSaveEditor.Forms;

/// <summary>
/// Main application form for the Xiuzhen Idle Save Editor.
/// Provides tabbed editing for:
///   - save.dat  (cultivation layers, talents, resources)
///   - svjy.dat  (sect disciples)
///   - svlz.dat  (farm + souls)
///   - svzb.dat  (lifestones)
/// </summary>
public class MainForm : Form
{
    // ── State ────────────────────────────────────────────────────────────────
    private string _saveFolder = "";
    private int _saveSlot = 0;

    private List<SaveRecord> _saveRecords = new();
    private List<Disciple> _disciples = new();
    private List<SoulRecord> _souls = new();
    private List<LifestoneRecord> _lifestones = new();
    private List<InventoryRecord> _inventory = new();
    private List<SectRecord> _sects = new();

    private ItemDatabase _itemDb = null!;

    // ── Top Controls
    private TextBox _folderBox = null!;
    private ComboBox _slotBox = null!;
    private Label _statusLabel = null!;

    // ── Tabs ─────────────────────────────────────────────────────────────────
    private TabControl _tabs = null!;

    // Main Save tab controls
    private DataGridView _cultivationGrid = null!;
    private Panel _statsPanel = null!;
    private Dictionary<string, TextBox> _statBoxes = new();

    // Disciples tab
    private DataGridView _disciplesGrid = null!;

    // Souls tab
    private DataGridView _soulsGrid = null!;

    // Lifestones tab
    private DataGridView _lifestonesGrid = null!;

    // Inventory tab
    private DataGridView _inventoryGrid = null!;
    private DataGridView _sectGrid = null!;

    public MainForm()
    {
        _itemDb = ItemDatabase.LoadEmbedded();
        InitializeUI();
    }

    // ── UI Construction ───────────────────────────────────────────────────────

    private void InitializeUI()
    {
        Text = "Xiuzhen Idle Save Editor";
        Size = new Size(1100, 750);
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9f);

        // ── Top panel ────────────────────────────────────────────────────────
        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            Padding = new Padding(8)
        };

        // Use a TableLayoutPanel so the folder textbox stretches with the form
        var topTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2,
            AutoSize = false
        };
        topTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));  // label
        topTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f)); // stretch
        topTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));  // btn1
        topTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));  // btn2
        topTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
        topTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));

        // Row 1: folder selector
        topTable.Controls.Add(new Label { Text = "Save Folder:", Width = 85, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);

        _folderBox = new TextBox { Dock = DockStyle.Fill };
        topTable.Controls.Add(_folderBox, 1, 0);

        var browseBtn = new Button { Text = "Browse…", Width = 80, Height = 26 };
        browseBtn.Click += BrowseFolder_Click;
        topTable.Controls.Add(browseBtn, 2, 0);
        topTable.SetColumnSpan(browseBtn, 2);

        // Row 2: slot selector + load/save
        topTable.Controls.Add(new Label { Text = "Save Slot:", Width = 85, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);

        var row2Flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0)
        };
        _slotBox = new ComboBox { Width = 60, DropDownStyle = ComboBoxStyle.DropDownList };
        for (int i = 0; i <= 9; i++) _slotBox.Items.Add(i.ToString());
        _slotBox.SelectedIndex = 0;
        row2Flow.Controls.Add(_slotBox);

        var loadBtn = new Button { Text = "Load", Width = 80, Height = 26 };
        loadBtn.Click += LoadSave_Click;
        row2Flow.Controls.Add(loadBtn);

        var saveBtn = new Button { Text = "Save Changes", Width = 110, Height = 26 };
        saveBtn.Click += SaveChanges_Click;
        row2Flow.Controls.Add(saveBtn);

        topTable.Controls.Add(row2Flow, 1, 1);
        topTable.SetColumnSpan(row2Flow, 3);

        topPanel.Controls.Add(topTable);

        // ── Status bar ───────────────────────────────────────────────────────
        var statusBar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 26,
            BackColor = SystemColors.ControlDark
        };
        _statusLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Ready — select a save folder and click Load.",
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.White,
            Padding = new Padding(4, 0, 0, 0)
        };
        statusBar.Controls.Add(_statusLabel);

        // ── TabControl ───────────────────────────────────────────────────────
        _tabs = new TabControl { Dock = DockStyle.Fill };

        _tabs.TabPages.Add(BuildMainSaveTab());
        _tabs.TabPages.Add(BuildDisciplesTab());
        _tabs.TabPages.Add(BuildSectTab());
        _tabs.TabPages.Add(BuildSoulsTab());
        _tabs.TabPages.Add(BuildLifestonesTab());
        _tabs.TabPages.Add(BuildInventoryTab());

        Controls.Add(_tabs);
        Controls.Add(statusBar);
        Controls.Add(topPanel);
    }

    // ── Tab: Main Save ────────────────────────────────────────────────────────

    private TabPage BuildMainSaveTab()
    {
        var page = new TabPage("Main Save (save.dat)");
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 260,
            Panel1MinSize = 160,
            Panel2MinSize = 120
        };

        // Top: cultivation techniques grid
        var cultLabel = new Label
        {
            Text = "Cultivation Techniques  (Technique ID → Layer & Mystic Realm Max Floor)",
            Dock = DockStyle.Top,
            Height = 22,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };

        _cultivationGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = SystemColors.Window,
            ScrollBars = ScrollBars.Both
        };
        _cultivationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID",      HeaderText = "ID",       Width = 40,  ReadOnly = true });
        _cultivationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name",    HeaderText = "Technique",Width = 120, ReadOnly = true });
        _cultivationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Layer",   HeaderText = "Layer" });
        _cultivationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Mystic",  HeaderText = "Mystic Realm", Width = 130, ReadOnly = true });
        _cultivationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Floor",   HeaderText = "Max Floor" });

        split.Panel1.Controls.Add(_cultivationGrid);
        split.Panel1.Controls.Add(cultLabel);

        // Bottom: stats (money, talents, experiences)
        _statsPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        BuildStatsPanel();
        split.Panel2.Controls.Add(_statsPanel);

        page.Controls.Add(split);
        return page;
    }

    private void BuildStatsPanel()
    {
        _statsPanel.Controls.Clear();
        _statBoxes.Clear();

        // Use a TableLayoutPanel for a clean 2-column layout
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(6)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

        var sections = new (string header, (string key, string label)[] fields)[]
        {
            ("── Player Resources ──", new[]
            {
                ("Money",             "Money (ID17)"),
                ("SoulCrystal",       "Soul Crystal (ID15)"),
                ("CultivationValue",  "Cultivation Value (ID16)"),
            }),
            ("── Player Talents ──", new[]
            {
                ("TalentHealth",   "Health Talent (ID5)"),
                ("TalentAttack",   "Attack Talent (ID6)"),
                ("TalentDefense",  "Defense Talent (ID7)"),
                ("TalentOverall",  "Overall Talent (ID8)"),
                ("TalentChance",   "Chance Talent (ID9)"),
            }),
            ("── Pet Stats ──", new[]
            {
                ("PetExp",           "Pet EXP (ID3)"),
                ("PetHealthTalent",  "Pet Health Talent (ID4)"),
                ("PetAttackTalent",  "Pet Attack Talent (ID5)"),
                ("PetDefenseTalent", "Pet Defense Talent (ID6)"),
            }),
            ("── Skill Experience ──", new[]
            {
                ("ExpGathering", "Gathering Exp (ID24)"),
                ("ExpMining",    "Mining Exp (ID26)"),
                ("ExpForging",   "Forging Exp (ID28)"),
                ("ExpAlchemy",   "Alchemy Exp (ID30)"),
                ("ExpTaming",    "Taming Exp (ID32)"),
            })
        };

        int col = 0;
        foreach (var (header, fields) in sections)
        {
            var group = new GroupBox
            {
                Text = header,
                Dock = DockStyle.Fill,
                Padding = new Padding(6),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            var inner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = fields.Length,
                AutoSize = true
            };
            inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
            inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));

            foreach (var (key, lbl) in fields)
            {
                inner.Controls.Add(new Label
                {
                    Text = lbl,
                    Font = new Font("Segoe UI", 8.5f),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleRight,
                    AutoSize = false
                });
                var tb = new TextBox { Dock = DockStyle.Fill, Width = 100 };
                _statBoxes[key] = tb;
                inner.Controls.Add(tb);
            }

            group.Controls.Add(inner);
            table.Controls.Add(group, col % 2, col / 2);
            col++;
        }

        _statsPanel.Controls.Add(table);
    }

    // ── Tab: Disciples ────────────────────────────────────────────────────────

    private TabPage BuildDisciplesTab()
    {
        var page = new TabPage("Disciples (svjy.dat)");

        var info = new Label
        {
            Text = "Talent grades: C=1-20 | B=21-40 | A=41-70 | S=71-100 | SS=101-140 | SSS=141-150",
            Dock = DockStyle.Top,
            Height = 22,
            ForeColor = Color.DarkBlue
        };

        _disciplesGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = SystemColors.Window,
            ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText,
            ScrollBars = ScrollBars.Both
        };

        AddDiscipleColumns();

        page.Controls.Add(_disciplesGrid);
        page.Controls.Add(info);
        return page;
    }

    private void AddDiscipleColumns()
    {
        foreach (var (name, header, ro) in DiscipleColumns())
        {
            _disciplesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                ReadOnly = ro
            });
        }
    }

    private static IEnumerable<(string name, string header, bool readOnly)> DiscipleColumns()
    {
        var columns = new List<(string name, string header, bool readOnly)>
        {
            ("Realm",       "Realm",           false),
            ("FamilyName",  "Family Name",     false),
            ("Name",        "Name",            false),
            ("UUID",        "UUID",            true),
            ("QiSense",     "Qi-Sense",        false),
            ("QiGrade",     "QS Grade",        true),
            ("God",         "God Sense",       false),
            ("GodGrade",    "God Grade",       true),
            ("Roots",       "Roots",           false),
            ("RootsGrade",  "Roots Grade",     true),
            ("Talent",      "Talent",          false),
            ("TalentGrade", "Talent Grade",    true),
            ("Chance",      "Chance",          false),
            ("ChanceGrade", "Chance Grade",    true),
            ("Building",    "Building",        false),
            ("Herbs",       "Herbs",           false),
            ("Mining",      "Mining",          false),
            ("Hunting",     "Hunting",         false),
            ("Taming",      "Taming",          false),
            ("External",    "External",        false),
            ("Alchemy",     "Alchemy",         false),
            ("Weapon",      "Weapon",          false),
            ("Position",    "Position",        false),
            ("Task",        "Task",            false),
            ("Unknown19",   "Raw 19",          false),
            ("Unknown20",   "Raw 20",          false),
            ("BuildTalent",    "Build Talent",    false),
            ("BuildGrade",     "Build Grade",     true),
            ("HerbsTalent",    "Herbs Talent",    false),
            ("HerbsGrade",     "Herbs Grade",     true),
            ("MineTalent",     "Mine Talent",     false),
            ("MineGrade",      "Mine Grade",      true),
            ("HuntTalent",     "Hunt Talent",     false),
            ("HuntGrade",      "Hunt Grade",      true),
            ("TameTalent",     "Tame Talent",     false),
            ("TameGrade",      "Tame Grade",      true),
            ("ExternalTalent", "External Talent", false),
            ("ExternalGrade",  "External Grade",  true),
            ("DanTalent",      "Dan Talent",      false),
            ("DanGrade",       "Dan Grade",       true),
            ("WeaponTalent",   "Weapon Talent",   false),
            ("WeaponGrade",    "Weapon Grade",    true),
        };

        for (int i = 29; i <= 50; i++)
            columns.Add(($"Raw{i}", $"Raw {i}", false));

        return columns;
    }

    private static object[] DiscipleDisplayValues(Disciple disciple)
    {
        var values = new List<object>
        {
            disciple.Realm, disciple.FamilyName, disciple.Name, disciple.UUID,
            disciple.QiSense, disciple.QiSenseGradeStr,
            disciple.God, disciple.GodGradeStr,
            disciple.Roots, disciple.RootsGradeStr,
            disciple.Talent, disciple.TalentGradeStr,
            disciple.Chance, disciple.ChanceGradeStr,
            disciple.Building, disciple.Herbs, disciple.Mining, disciple.Hunting,
            disciple.Taming, disciple.External, disciple.Alchemy, disciple.Weapon,
            disciple.Position, disciple.Task,
            disciple.Unknown1, disciple.Unknown2,
            disciple.BuildTalent, disciple.BuildTalentGradeStr,
            disciple.HerbsTalent, disciple.HerbsTalentGradeStr,
            disciple.MineTalent, disciple.MineTalentGradeStr,
            disciple.HuntTalent, disciple.HuntTalentGradeStr,
            disciple.TameTalent, disciple.TameTalentGradeStr,
            disciple.ExternalTalent, disciple.ExternalTalentGradeStr,
            disciple.DanTalent, disciple.DanTalentGradeStr,
            disciple.WeaponTalent, disciple.WeaponTalentGradeStr,
        };

        for (int i = 29; i <= 50; i++)
            values.Add(disciple.GetRaw(i));

        return values.ToArray();
    }

    private static readonly (int ModelIndex, string GridCol)[] EditableDiscipleColumns =
    {
        (0, "Realm"),
        (1, "FamilyName"),
        (2, "Name"),
        (4, "QiSense"),
        (5, "God"),
        (6, "Roots"),
        (7, "Talent"),
        (8, "Chance"),
        (9, "Building"),
        (10, "Herbs"),
        (11, "Mining"),
        (12, "Hunting"),
        (13, "Taming"),
        (14, "External"),
        (15, "Alchemy"),
        (16, "Weapon"),
        (17, "Position"),
        (18, "Task"),
        (19, "Unknown19"),
        (20, "Unknown20"),
        (21, "BuildTalent"),
        (22, "HerbsTalent"),
        (23, "MineTalent"),
        (24, "HuntTalent"),
        (25, "TameTalent"),
        (26, "ExternalTalent"),
        (27, "DanTalent"),
        (28, "WeaponTalent"),
        (29, "Raw29"),
        (30, "Raw30"),
        (31, "Raw31"),
        (32, "Raw32"),
        (33, "Raw33"),
        (34, "Raw34"),
        (35, "Raw35"),
        (36, "Raw36"),
        (37, "Raw37"),
        (38, "Raw38"),
        (39, "Raw39"),
        (40, "Raw40"),
        (41, "Raw41"),
        (42, "Raw42"),
        (43, "Raw43"),
        (44, "Raw44"),
        (45, "Raw45"),
        (46, "Raw46"),
        (47, "Raw47"),
        (48, "Raw48"),
        (49, "Raw49"),
        (50, "Raw50"),
    };

    private TabPage BuildSectTab()
    {
        var page = new TabPage("Sect Data (svmp.dat)");

        var info = new Label
        {
            Text = "Raw c2array view for sect data. Column names use the original group/subvalue positions.",
            Dock = DockStyle.Top,
            Height = 22,
            ForeColor = Color.DarkBlue
        };

        _sectGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = SystemColors.Window,
            ScrollBars = ScrollBars.Both
        };

        page.Controls.Add(_sectGrid);
        page.Controls.Add(info);
        return page;
    }

    private void EnsureSectColumns()
    {
        _sectGrid.Columns.Clear();
        _sectGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "RowIndex",
            HeaderText = "Row",
            ReadOnly = true,
        });

        int maxGroups = 0;
        int maxDepth = 0;
        foreach (var sect in _sects)
        {
            maxGroups = Math.Max(maxGroups, sect.Groups.Count);
            foreach (var group in sect.Groups)
                maxDepth = Math.Max(maxDepth, group.Count);
        }

        for (int group = 0; group < maxGroups; group++)
        {
            for (int sub = 0; sub < maxDepth; sub++)
            {
                var column = new DataGridViewTextBoxColumn
                {
                    Name = SectColumnName(group, sub),
                    HeaderText = $"G{group:D2}[{sub}]",
                    ReadOnly = false,
                    Tag = (group, sub)
                };
                _sectGrid.Columns.Add(column);
            }
        }
    }

    private static string SectColumnName(int group, int sub) => $"G{group:D2}_{sub}";
    private TabPage BuildSoulsTab()
    {
        var page = new TabPage("Souls/Farm (svlz.dat)");

        var info = new Label
        {
            Text = "Soul stats (1-120) are multiplied externally (e.g. ST2=34 → Resonance 3400).",
            Dock = DockStyle.Top,
            Height = 22,
            ForeColor = Color.DarkBlue
        };

        _soulsGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = SystemColors.Window,
            ScrollBars = ScrollBars.Both
        };

        foreach (var (name, header, ro) in SoulColumns())
            _soulsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                ReadOnly = ro
            });

        page.Controls.Add(_soulsGrid);
        page.Controls.Add(info);
        return page;
    }

    private static (string, string, bool)[] SoulColumns() => new[]
    {
        ("SoulID",      "Soul ID",       true),
        ("SoulName",    "Soul Name",     true),
        ("FruitID",     "Fruit ID",      false),
        ("FruitName",   "Fruit Name",    true),
        ("Spirit",      "Spirit",        false),
        ("Resonance",   "Resonance",     false),
        ("Strength",    "Strength",      false),
        ("Stability",   "Stability",     false),
        ("Fortune",     "Fortune",       false),
        ("LifespanUsed","Lifespan Used", false),
        ("Lifespan",    "Lifespan",      false),
        ("FruitAge",    "Fruit Age",     false),
        ("HarvestName", "Harvest Name",  false),
    };

    // ── Tab: Lifestones ───────────────────────────────────────────────────────

    private TabPage BuildLifestonesTab()
    {
        var page = new TabPage("Lifestones (svzb.dat)");

        _lifestonesGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = SystemColors.Window,
            ScrollBars = ScrollBars.Both
        };

        foreach (var (name, header, ro) in LifestoneColumns())
            _lifestonesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                ReadOnly = ro
            });

        page.Controls.Add(_lifestonesGrid);
        return page;
    }

    private static (string, string, bool)[] LifestoneColumns() => new[]
    {
        ("LID",    "Lifestone ID", true),
        ("Level",  "Level",        false),
        ("EID1",   "Effect 1 ID",  false),
        ("EID2",   "Effect 2 ID",  false),
        ("EID3",   "Effect 3 ID",  false),
        ("EID4",   "Effect 4 ID",  false),
        ("EID5",   "Effect 5 ID",  false),
        ("EP1",    "Effect 1 %",   false),
        ("EP2",    "Effect 2 %",   false),
        ("EP3",    "Effect 3 %",   false),
        ("EP4",    "Effect 4 %",   false),
        ("EP5",    "Effect 5 %",   false),
    };

    // ── Tab: Inventory ────────────────────────────────────────────────────────

    private TabPage BuildInventoryTab()
    {
        var page = new TabPage("Inventory (svwp.dat)");

        _inventoryGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = SystemColors.Window,
            ScrollBars = ScrollBars.Both
        };

        foreach (var (name, header, ro) in InventoryColumns())
            _inventoryGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                ReadOnly = ro
            });

        page.Controls.Add(_inventoryGrid);
        return page;
    }

    private static (string, string, bool)[] InventoryColumns() => new[]
    {
        ("ItemID",   "Item ID",   true),
        ("ItemName", "Item Name", true),
        ("Category", "Category",  true),
        ("Quantity", "Quantity",   false),
    };

    // ── Event Handlers

    private void BrowseFolder_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Select the Xiuzhen Idle save folder",
            UseDescriptionForTitle = true
        };
        if (dlg.ShowDialog() == DialogResult.OK)
            _folderBox.Text = dlg.SelectedPath;
    }

    private void LoadSave_Click(object? sender, EventArgs e)
    {
        string folder = _folderBox.Text.Trim();
        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
        {
            SetStatus("Error: folder not found.", error: true);
            return;
        }

        _saveFolder = folder;
        _saveSlot = int.TryParse(_slotBox.SelectedItem?.ToString(), out int slot) ? slot : 0;

        try
        {
            LoadMainSave();
            LoadDisciples();
            LoadSectData();
            LoadSouls();
            LoadLifestones();
            LoadInventory();
            SetStatus($"Slot {_saveSlot} loaded from: {_saveFolder}");
        }
        catch (Exception ex)
        {
            SetStatus($"Load error: {ex.Message}", error: true);
        }
    }

    private void SaveChanges_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_saveFolder))
        {
            SetStatus("No save folder loaded.", error: true);
            return;
        }

        try
        {
            SaveMainSave();
            SaveDisciples();
            SaveSectData();
            SaveSouls();
            SaveLifestones();
            SaveInventory();
            SetStatus("All changes saved successfully.");
        }
        catch (Exception ex)
        {
            SetStatus($"Save error: {ex.Message}", error: true);
        }
    }

    // ── Load Helpers ──────────────────────────────────────────────────────────

    private string SlotFile(string suffix) =>
        Path.Combine(_saveFolder, $"{_saveSlot}{suffix}");

    private void LoadMainSave()
    {
        string path = SlotFile("save.dat");
        _saveRecords = SaveDataParser.Parse(path);

        // Populate cultivation grid
        _cultivationGrid.Rows.Clear();
        foreach (int id in SaveDataParser.CultivationIds)
        {
            var rec = SaveDataParser.FindById(_saveRecords, id);
            string layer = rec?.GetValue(SaveDataParser.PosLayer) ?? "0";
            string floor = rec?.GetValue(SaveDataParser.PosFloor) ?? "0";
            string mystic = SaveDataParser.MysticNames.TryGetValue(id, out string? mn) ? mn : $"Mystic {id}";
            string name = SaveDataParser.CultivationNames.TryGetValue(id, out string? cn) ? cn : $"Cultivation {id}";
            _cultivationGrid.Rows.Add(id, name, layer, mystic, floor);
        }

        // Populate stat text boxes
        void SetBox(string key, int recordId, int pos)
        {
            if (_statBoxes.TryGetValue(key, out var tb))
                tb.Text = SaveDataParser.FindById(_saveRecords, recordId)?.GetValue(pos) ?? "0";
        }

        SetBox("Money",            17, SaveDataParser.PosMainValue);
        SetBox("SoulCrystal",      15, SaveDataParser.PosMainValue);
        SetBox("CultivationValue", 16, SaveDataParser.PosMainValue);

        SetBox("TalentHealth",  5, SaveDataParser.PosMainValue);
        SetBox("TalentAttack",  6, SaveDataParser.PosMainValue);
        SetBox("TalentDefense", 7, SaveDataParser.PosMainValue);
        SetBox("TalentOverall", 8, SaveDataParser.PosMainValue);
        SetBox("TalentChance",  9, SaveDataParser.PosMainValue);

        SetBox("PetExp",           3, SaveDataParser.PosSpecial);
        SetBox("PetHealthTalent",  4, SaveDataParser.PosSpecial);
        SetBox("PetAttackTalent",  5, SaveDataParser.PosSpecial);
        SetBox("PetDefenseTalent", 6, SaveDataParser.PosSpecial);

        SetBox("ExpGathering", 24, SaveDataParser.PosMainValue);
        SetBox("ExpMining",    26, SaveDataParser.PosMainValue);
        SetBox("ExpForging",   28, SaveDataParser.PosMainValue);
        SetBox("ExpAlchemy",   30, SaveDataParser.PosMainValue);
        SetBox("ExpTaming",    32, SaveDataParser.PosMainValue);
    }

    private void LoadDisciples()
    {
        string path = SlotFile("svjy.dat");
        _disciples = DiscipleParser.Parse(path);

        _disciplesGrid.Rows.Clear();
        foreach (var d in _disciples.Where(d => !d.IsEmpty))
        {
            int rowIndex = _disciplesGrid.Rows.Add(DiscipleDisplayValues(d));
            _disciplesGrid.Rows[rowIndex].Tag = d;
        }
    }

    private void LoadSectData()
    {
        string path = SlotFile("svmp.dat");
        _sects = SectParser.Parse(path);

        EnsureSectColumns();
        _sectGrid.Rows.Clear();

        for (int recordIndex = 0; recordIndex < _sects.Count; recordIndex++)
        {
            var sect = _sects[recordIndex];
            if (sect.IsEmpty)
                continue;

            var values = new object[_sectGrid.Columns.Count];
            values[0] = recordIndex;
            foreach (DataGridViewColumn column in _sectGrid.Columns)
            {
                if (column.Tag is ValueTuple<int, int> mapping)
                    values[column.Index] = sect.GetSub(mapping.Item1, mapping.Item2);
            }

            int rowIndex = _sectGrid.Rows.Add(values);
            _sectGrid.Rows[rowIndex].Tag = sect;
        }
    }
    private void LoadSouls()
    {
        string path = SlotFile("svlz.dat");
        _souls = SoulParser.Parse(path);

        _soulsGrid.Rows.Clear();
        foreach (var s in _souls.Where(s => !s.IsEmpty))
        {
            int.TryParse(s.SoulId, out int soulId);
            int.TryParse(s.FruitId, out int fruitId);
            int rowIndex = _soulsGrid.Rows.Add(
                s.SoulId, _itemDb.GetName(soulId),
                s.FruitId, fruitId > 0 ? _itemDb.GetName(fruitId) : "",
                s.Spirit, s.Resonance, s.Strength, s.Stability, s.Fortune,
                s.LifespanUsed, s.Lifespan,
                s.FruitAge, s.HarvestName
            );
            _soulsGrid.Rows[rowIndex].Tag = s;
        }
    }

    private void LoadLifestones()
    {
        string path = SlotFile("svzb.dat");
        _lifestones = LifestoneParser.Parse(path);

        _lifestonesGrid.Rows.Clear();
        foreach (var ls in _lifestones.Where(ls => !ls.IsEmpty))
        {
            int rowIndex = _lifestonesGrid.Rows.Add(
                ls.LifestoneId, ls.Level,
                ls.Effect1Id, ls.Effect2Id, ls.Effect3Id, ls.Effect4Id, ls.Effect5Id,
                ls.Effect1Pct, ls.Effect2Pct, ls.Effect3Pct, ls.Effect4Pct, ls.Effect5Pct
            );
            _lifestonesGrid.Rows[rowIndex].Tag = ls;
        }
    }

    private void LoadInventory()
    {
        string path = SlotFile("svwp.dat");
        _inventory = InventoryParser.Parse(path);

        _inventoryGrid.Rows.Clear();
        foreach (var inv in _inventory.Where(inv => !inv.IsEmpty))
        {
            int.TryParse(inv.ItemId, out int itemId);
            var item = _itemDb.GetById(itemId);
            int rowIndex = _inventoryGrid.Rows.Add(
                inv.ItemId,
                item?.Name ?? $"Unknown ({itemId})",
                item?.Category ?? "",
                inv.Quantity
            );
            _inventoryGrid.Rows[rowIndex].Tag = inv;
        }
    }

    // ── Save Helpers

    private void SaveMainSave()
    {
        string path = SlotFile("save.dat");
        if (!File.Exists(path)) return;

        // Write back cultivation grid changes
        foreach (DataGridViewRow row in _cultivationGrid.Rows)
        {
            if (!int.TryParse(row.Cells["ID"].Value?.ToString(), out int id)) continue;
            var rec = SaveDataParser.FindById(_saveRecords, id);
            if (rec == null) continue;
            rec.SetValue(SaveDataParser.PosLayer,  row.Cells["Layer"].Value?.ToString() ?? rec.GetValue(SaveDataParser.PosLayer));
            rec.SetValue(SaveDataParser.PosFloor, row.Cells["Floor"].Value?.ToString() ?? rec.GetValue(SaveDataParser.PosFloor));
        }

        // Write back stat box changes
        void ReadBox(string key, int recordId, int pos)
        {
            if (!_statBoxes.TryGetValue(key, out var tb)) return;
            var rec = SaveDataParser.FindById(_saveRecords, recordId);
            rec?.SetValue(pos, tb.Text.Trim());
        }

        ReadBox("Money",            17, SaveDataParser.PosMainValue);
        ReadBox("SoulCrystal",      15, SaveDataParser.PosMainValue);
        ReadBox("CultivationValue", 16, SaveDataParser.PosMainValue);

        ReadBox("TalentHealth",  5, SaveDataParser.PosMainValue);
        ReadBox("TalentAttack",  6, SaveDataParser.PosMainValue);
        ReadBox("TalentDefense", 7, SaveDataParser.PosMainValue);
        ReadBox("TalentOverall", 8, SaveDataParser.PosMainValue);
        ReadBox("TalentChance",  9, SaveDataParser.PosMainValue);

        ReadBox("PetExp",           3, SaveDataParser.PosSpecial);
        ReadBox("PetHealthTalent",  4, SaveDataParser.PosSpecial);
        ReadBox("PetAttackTalent",  5, SaveDataParser.PosSpecial);
        ReadBox("PetDefenseTalent", 6, SaveDataParser.PosSpecial);

        ReadBox("ExpGathering", 24, SaveDataParser.PosMainValue);
        ReadBox("ExpMining",    26, SaveDataParser.PosMainValue);
        ReadBox("ExpForging",   28, SaveDataParser.PosMainValue);
        ReadBox("ExpAlchemy",   30, SaveDataParser.PosMainValue);
        ReadBox("ExpTaming",    32, SaveDataParser.PosMainValue);

        SaveDataParser.Save(path, _saveRecords);
    }

    private void SaveDisciples()
    {
        string path = SlotFile("svjy.dat");
        if (!File.Exists(path)) return;

        foreach (DataGridViewRow row in _disciplesGrid.Rows)
        {
            if (row.Tag is not Disciple disciple)
                continue;

            foreach (var (modelIndex, gridCol) in EditableDiscipleColumns)
            {
                string? val = row.Cells[gridCol].Value?.ToString();
                if (val != null)
                    disciple.Set(modelIndex, val);
            }
        }

        DiscipleParser.Save(path, _disciples);
    }

    private void SaveSectData()
    {
        string path = SlotFile("svmp.dat");
        if (!File.Exists(path)) return;

        foreach (DataGridViewRow row in _sectGrid.Rows)
        {
            if (row.Tag is not SectRecord sect)
                continue;

            foreach (DataGridViewColumn column in _sectGrid.Columns)
            {
                if (column.Tag is not ValueTuple<int, int> mapping)
                    continue;

                string? val = row.Cells[column.Index].Value?.ToString();
                if (val != null)
                    sect.SetSub(mapping.Item1, mapping.Item2, val);
            }
        }

        SectParser.Save(path, _sects);
    }
    private void SaveSouls()
    {
        string path = SlotFile("svlz.dat");
        if (!File.Exists(path)) return;

        var colMappings = new (string gridCol, Action<SoulRecord, string> setter)[]
        {
            ("FruitID",     (s, v) => s.SetSub(1, 2, v)),
            ("Spirit",      (s, v) => s.SetSub(2, 0, v)),
            ("Resonance",   (s, v) => s.SetSub(3, 0, v)),
            ("Strength",    (s, v) => s.SetSub(4, 0, v)),
            ("Stability",   (s, v) => s.SetSub(5, 0, v)),
            ("Fortune",     (s, v) => s.SetSub(6, 0, v)),
            ("LifespanUsed",(s, v) => s.SetSub(2, 2, v)),
            ("Lifespan",    (s, v) => s.SetSub(3, 2, v)),
            ("FruitAge",    (s, v) => s.SetSub(4, 2, v)),
            ("HarvestName", (s, v) => s.SetSub(6, 2, v)),
        };

        foreach (DataGridViewRow row in _soulsGrid.Rows)
        {
            if (row.Tag is not SoulRecord soul)
                continue;

            foreach (var (col, setter) in colMappings)
            {
                string? val = row.Cells[col].Value?.ToString();
                if (val != null) setter(soul, val);
            }
        }

        SoulParser.Save(path, _souls);
    }

    private void SaveLifestones()
    {
        string path = SlotFile("svzb.dat");
        if (!File.Exists(path)) return;

        var colMappings = new (string gridCol, Action<LifestoneRecord, string> setter)[]
        {
            ("Level", (l, v) => l.SetSub(1, 1, v)),
            ("EID1",  (l, v) => l.SetSub(3, 1, v)),
            ("EID2",  (l, v) => l.SetSub(4, 1, v)),
            ("EID3",  (l, v) => l.SetSub(5, 1, v)),
            ("EID4",  (l, v) => l.SetSub(6, 1, v)),
            ("EID5",  (l, v) => l.SetSub(8, 1, v)),
            ("EP1",   (l, v) => l.SetSub(9, 1, v)),
            ("EP2",   (l, v) => l.SetSub(10, 1, v)),
            ("EP3",   (l, v) => l.SetSub(11, 1, v)),
            ("EP4",   (l, v) => l.SetSub(12, 1, v)),
            ("EP5",   (l, v) => l.SetSub(13, 1, v)),
        };

        foreach (DataGridViewRow row in _lifestonesGrid.Rows)
        {
            if (row.Tag is not LifestoneRecord stone)
                continue;

            foreach (var (col, setter) in colMappings)
            {
                string? val = row.Cells[col].Value?.ToString();
                if (val != null) setter(stone, val);
            }
        }

        LifestoneParser.Save(path, _lifestones);
    }

    private void SaveInventory()
    {
        string path = SlotFile("svwp.dat");
        if (!File.Exists(path)) return;

        foreach (DataGridViewRow row in _inventoryGrid.Rows)
        {
            if (row.Tag is not InventoryRecord inventoryRecord)
                continue;

            string? qty = row.Cells["Quantity"].Value?.ToString();
            if (qty != null)
                inventoryRecord.SetSub(1, 0, qty);
        }

        InventoryParser.Save(path, _inventory);
    }

    // ── Utilities

    private void SetStatus(string message, bool error = false)
    {
        _statusLabel.Text = message;
        _statusLabel.BackColor = error ? Color.DarkRed : SystemColors.ControlDark;
    }
}

