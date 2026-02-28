# Xiuzhen-Idle-Save-Editor

A C# WinForms save editor for the game **Xiuzhen Idle**.

## Features

- **Main Save (`save.dat`)** — Edit cultivation technique layers and mystic realm max floors, player resources (Money, Soul Crystal, Cultivation Value), player talents (Health, Attack, Defense, Overall, Chance), pet stats (EXP and talents), and skill experience values (Gathering, Mining, Forging, Alchemy, Taming).
- **Sect Disciples (`svjy.dat`)** — View and edit all disciple attributes including realm, name, talent values, and positions. Talent grades (C/B/A/S/SS/SSS) are displayed automatically.
- **Souls/Farm (`svlz.dat`)** — View and edit soul records including Spirit/Resonance/Strength/Stability/Fortune stats, lifespans, and harvest names.
- **Lifestones (`svzb.dat`)** — View and edit lifestone records including level, effect IDs, and effect percentages.

## Save File Format

Save files are stored in the game's save directory. The number prefix (e.g. `0`, `1`, `2`) corresponds to the save slot:

| File | Contents |
|------|----------|
| `{slot}save.dat` | Main save data (cultivation, talents, money) |
| `{slot}svjy.dat` | Sect disciples |
| `{slot}svlz.dat` | Farm + Souls |
| `{slot}svzb.dat` | Lifestones |
| `{slot}svwp.dat` | Inventory |
| `{slot}svmp.dat` | Sect data |
| `1global.dat`    | Global game data |

## Usage

1. Launch `XiuzhenSaveEditor.exe`
2. Click **Browse…** to select your game save folder
3. Select the save **Slot** (0 = first save, 1 = second, etc.)
4. Click **Load** to read all supported save files
5. Edit values in the tabs
6. Click **Save Changes** to write modifications back to disk

> ⚠️ **Always back up your save files before editing!**

## Talent Grades

Talent values in `svjy.dat` are scored 1–150:

| Grade | Range |
|-------|-------|
| C | 1 – 20 |
| B | 21 – 40 |
| A | 41 – 70 |
| S | 71 – 100 |
| SS | 101 – 140 |
| SSS | 141 – 150 |

## Building

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) with Windows targeting.

```bash
# Build
dotnet build XiuzhenSaveEditor/XiuzhenSaveEditor.csproj

# Run tests
dotnet test XiuzhenSaveEditor.Tests/XiuzhenSaveEditor.Tests.csproj
```

## Project Structure

```
XiuzhenSaveEditor.Core/        Cross-platform library: models + parsers
  Models/
    SaveRecord.cs              Row in save.dat
    Disciple.cs                Sect disciple (svjy.dat)
    SoulRecord.cs              Soul/Farm entry (svlz.dat)
    LifestoneRecord.cs         Lifestone entry (svzb.dat)
  Parsers/
    BracketParser.cs           Bracket-format parser utility
    SaveDataParser.cs          Parses save.dat
    DiscipleParser.cs          Parses svjy.dat
    SoulParser.cs              Parses svlz.dat
    LifestoneParser.cs         Parses svzb.dat

XiuzhenSaveEditor/             WinForms GUI application
  Forms/
    MainForm.cs                Tabbed main window

XiuzhenSaveEditor.Tests/       xUnit test suite for Core library
```
