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

    // ── Top Controls ─────────────────────────────────────────────────────────
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

    public MainForm()
    {
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

        // Row 1: folder selector
        var row1 = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 36,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        row1.Controls.Add(new Label { Text = "Save Folder:", Width = 85, TextAlign = ContentAlignment.MiddleRight, Height = 24 });

        _folderBox = new TextBox { Width = 550, Height = 24 };
        row1.Controls.Add(_folderBox);

        var browseBtn = new Button { Text = "Browse…", Width = 80, Height = 26 };
        browseBtn.Click += BrowseFolder_Click;
        row1.Controls.Add(browseBtn);

        // Row 2: slot selector + load/save
        var row2 = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 36,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        row2.Controls.Add(new Label { Text = "Save Slot:", Width = 85, TextAlign = ContentAlignment.MiddleRight, Height = 24 });

        _slotBox = new ComboBox { Width = 60, DropDownStyle = ComboBoxStyle.DropDownList };
        for (int i = 0; i <= 9; i++) _slotBox.Items.Add(i.ToString());
        _slotBox.SelectedIndex = 0;
        row2.Controls.Add(_slotBox);

        var loadBtn = new Button { Text = "Load", Width = 80, Height = 26 };
        loadBtn.Click += LoadSave_Click;
        row2.Controls.Add(loadBtn);

        var saveBtn = new Button { Text = "Save Changes", Width = 110, Height = 26 };
        saveBtn.Click += SaveChanges_Click;
        row2.Controls.Add(saveBtn);

        topPanel.Controls.Add(row2);
        topPanel.Controls.Add(row1);

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
        _tabs.TabPages.Add(BuildSoulsTab());
        _tabs.TabPages.Add(BuildLifestonesTab());

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
            BackgroundColor = SystemColors.Window
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

        // Use a TableLayoutPanel for a clean 3-column layout
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            AutoSize = true,
            Padding = new Padding(6)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));

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
            table.Controls.Add(group, col % 3, col / 3);
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
            ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText
        };

        AddDiscipleColumns();

        page.Controls.Add(_disciplesGrid);
        page.Controls.Add(info);
        return page;
    }

    private void AddDiscipleColumns()
    {
        var columns = new[]
        {
            ("Realm",       "Realm",        false),
            ("FamilyName",  "Family Name",  false),
            ("Name",        "Name",         false),
            ("UUID",        "UUID",         true),
            ("QiSense",     "Qi-Sense",     false),
            ("QiGrade",     "QS Grade",     true),
            ("God",         "God Sense",    false),
            ("GodGrade",    "God Grade",    true),
            ("Roots",       "Roots",        false),
            ("RootsGrade",  "Roots Grade",  true),
            ("Talent",      "Talent",       false),
            ("TalentGrade", "Talent Grade", true),
            ("Chance",      "Chance",       false),
            ("ChanceGrade", "Chance Grade", true),
            ("Building",    "Building",     false),
            ("Herbs",       "Herbs",        false),
            ("Mining",      "Mining",       false),
            ("Hunting",     "Hunting",      false),
            ("Taming",      "Taming",       false),
            ("External",    "External",     false),
            ("Alchemy",     "Alchemy",      false),
            ("Weapon",      "Weapon",       false),
            ("Position",    "Position",     false),
            ("Task",        "Task",         false),
        };

        foreach (var (name, header, ro) in columns)
        {
            _disciplesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                ReadOnly = ro
            });
        }
    }

    // ── Tab: Souls ────────────────────────────────────────────────────────────

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
            BackgroundColor = SystemColors.Window
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
        ("FruitID",     "Fruit ID",      false),
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
            BackgroundColor = SystemColors.Window
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

    // ── Event Handlers ────────────────────────────────────────────────────────

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
        _saveSlot = _slotBox.SelectedIndex;

        try
        {
            LoadMainSave();
            LoadDisciples();
            LoadSouls();
            LoadLifestones();
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
            SaveSouls();
            SaveLifestones();
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
            string layer = rec?.GetValue(1) ?? "0";
            string floor = rec?.GetValue(24) ?? "0";
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

        SetBox("Money",            17, 22);
        SetBox("SoulCrystal",      15, 22);
        SetBox("CultivationValue", 16, 22);

        SetBox("TalentHealth",  5, 22);
        SetBox("TalentAttack",  6, 22);
        SetBox("TalentDefense", 7, 22);
        SetBox("TalentOverall", 8, 22);
        SetBox("TalentChance",  9, 22);

        SetBox("PetExp",           3, 17);
        SetBox("PetHealthTalent",  4, 17);
        SetBox("PetAttackTalent",  5, 17);
        SetBox("PetDefenseTalent", 6, 17);

        SetBox("ExpGathering", 24, 22);
        SetBox("ExpMining",    26, 22);
        SetBox("ExpForging",   28, 22);
        SetBox("ExpAlchemy",   30, 22);
        SetBox("ExpTaming",    32, 22);
    }

    private void LoadDisciples()
    {
        string path = SlotFile("svjy.dat");
        _disciples = DiscipleParser.Parse(path);

        _disciplesGrid.Rows.Clear();
        foreach (var d in _disciples)
        {
            _disciplesGrid.Rows.Add(
                d.Realm, d.FamilyName, d.Name, d.UUID,
                d.QiSense, d.QiSenseGradeStr,
                d.God, d.GodGradeStr,
                d.Roots, d.RootsGradeStr,
                d.Talent, d.TalentGradeStr,
                d.Chance, d.ChanceGradeStr,
                d.Building, d.Herbs, d.Mining, d.Hunting,
                d.Taming, d.External, d.Alchemy, d.Weapon,
                d.Position, d.Task
            );
        }
    }

    private void LoadSouls()
    {
        string path = SlotFile("svlz.dat");
        _souls = SoulParser.Parse(path);

        _soulsGrid.Rows.Clear();
        foreach (var s in _souls)
        {
            _soulsGrid.Rows.Add(
                s.SoulId, s.FruitId,
                s.Spirit, s.Resonance, s.Strength, s.Stability, s.Fortune,
                s.LifespanUsed, s.Lifespan,
                s.FruitAge, BracketParser.StripQuotes(s.HarvestName)
            );
        }
    }

    private void LoadLifestones()
    {
        string path = SlotFile("svzb.dat");
        _lifestones = LifestoneParser.Parse(path);

        _lifestonesGrid.Rows.Clear();
        foreach (var ls in _lifestones)
        {
            _lifestonesGrid.Rows.Add(
                ls.LifestoneId, ls.Level,
                ls.Effect1Id, ls.Effect2Id, ls.Effect3Id, ls.Effect4Id, ls.Effect5Id,
                ls.Effect1Pct, ls.Effect2Pct, ls.Effect3Pct, ls.Effect4Pct, ls.Effect5Pct
            );
        }
    }

    // ── Save Helpers ──────────────────────────────────────────────────────────

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
            rec.SetValue(1,  row.Cells["Layer"].Value?.ToString() ?? rec.GetValue(1));
            rec.SetValue(24, row.Cells["Floor"].Value?.ToString() ?? rec.GetValue(24));
        }

        // Write back stat box changes
        void ReadBox(string key, int recordId, int pos)
        {
            if (!_statBoxes.TryGetValue(key, out var tb)) return;
            var rec = SaveDataParser.FindById(_saveRecords, recordId);
            rec?.SetValue(pos, tb.Text.Trim());
        }

        ReadBox("Money",            17, 22);
        ReadBox("SoulCrystal",      15, 22);
        ReadBox("CultivationValue", 16, 22);

        ReadBox("TalentHealth",  5, 22);
        ReadBox("TalentAttack",  6, 22);
        ReadBox("TalentDefense", 7, 22);
        ReadBox("TalentOverall", 8, 22);
        ReadBox("TalentChance",  9, 22);

        ReadBox("PetExp",           3, 17);
        ReadBox("PetHealthTalent",  4, 17);
        ReadBox("PetAttackTalent",  5, 17);
        ReadBox("PetDefenseTalent", 6, 17);

        ReadBox("ExpGathering", 24, 22);
        ReadBox("ExpMining",    26, 22);
        ReadBox("ExpForging",   28, 22);
        ReadBox("ExpAlchemy",   30, 22);
        ReadBox("ExpTaming",    32, 22);

        SaveDataParser.Save(path, _saveRecords);
    }

    private void SaveDisciples()
    {
        string path = SlotFile("svjy.dat");
        if (!File.Exists(path)) return;

        // Sync grid editable columns back to model
        // Editable columns: Realm(0), FamilyName(1), Name(2), QiSense(4), God(5), Roots(6),
        //   Talent(7), Chance(8), Building(9), Herbs(10), Mining(11), Hunting(12),
        //   Taming(13), External(14), Alchemy(15), Weapon(16), Position(17), Task(18)
        int[] editableModelCols = { 0, 1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 };
        // Corresponding grid column names (skipping grade columns which are read-only)
        string[] editableGridCols = {
            "Realm", "FamilyName", "Name", "QiSense", "God", "Roots",
            "Talent", "Chance", "Building", "Herbs", "Mining", "Hunting",
            "Taming", "External", "Alchemy", "Weapon", "Position", "Task"
        };

        for (int i = 0; i < _disciples.Count && i < _disciplesGrid.Rows.Count; i++)
        {
            for (int j = 0; j < editableModelCols.Length; j++)
            {
                string? val = _disciplesGrid.Rows[i].Cells[editableGridCols[j]].Value?.ToString();
                if (val != null)
                    _disciples[i].Set(editableModelCols[j], val);
            }
        }

        DiscipleParser.Save(path, _disciples);
    }

    private void SaveSouls()
    {
        string path = SlotFile("svlz.dat");
        if (!File.Exists(path)) return;

        // Sync editable grid fields back to soul records
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
            ("HarvestName", (s, v) => s.SetSub(6, 2, v.StartsWith('"') ? v : $"\"{v}\"")),
        };

        for (int i = 0; i < _souls.Count && i < _soulsGrid.Rows.Count; i++)
        {
            foreach (var (col, setter) in colMappings)
            {
                string? val = _soulsGrid.Rows[i].Cells[col].Value?.ToString();
                if (val != null) setter(_souls[i], val);
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
            ("Level", (l, v) => l.SetSub(2, 1, v)),
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

        for (int i = 0; i < _lifestones.Count && i < _lifestonesGrid.Rows.Count; i++)
        {
            foreach (var (col, setter) in colMappings)
            {
                string? val = _lifestonesGrid.Rows[i].Cells[col].Value?.ToString();
                if (val != null) setter(_lifestones[i], val);
            }
        }

        LifestoneParser.Save(path, _lifestones);
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    private void SetStatus(string message, bool error = false)
    {
        _statusLabel.Text = message;
        _statusLabel.BackColor = error ? Color.DarkRed : SystemColors.ControlDark;
    }
}
