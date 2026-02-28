using XiuzhenSaveEditor.Forms;

namespace XiuzhenSaveEditor;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}