using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using CleanSweep.Interfaces;

internal static class FunctionalTests
{
    private static int passed, failed;
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly string Root = Path.Combine(Path.GetTempPath(), "CleanSweep-tests-" + Guid.NewGuid().ToString("N"));
    [STAThread]
    private static int Main()
    {
        Directory.CreateDirectory(Root);
        Control.CheckForIllegalCrossThreadCalls = true;
        Application.EnableVisualStyles();
        using (var host = new Form { ShowInTaskbar = false, Opacity = 0 })
        using (var log = new RichTextBox())
        {
            host.Controls.Add(log);
            host.Shown += async (sender, args) => {
                try { await Run(log); }
                catch (Exception ex) { failed++; Console.WriteLine("FAIL harness: " + ex); }
                finally { host.Close(); }
            };
            Application.Run(host);
        }
        // Delete only this run's randomly named, verified fixture tree.
        if (Path.GetFullPath(Root).StartsWith(Path.GetFullPath(Path.GetTempPath()) + "CleanSweep-tests-", StringComparison.OrdinalIgnoreCase))
            Directory.Delete(Root, true);
        Console.WriteLine("RESULT passed=" + passed + " failed=" + failed);
        return failed == 0 ? 0 : 1;
    }
    private static void Check(string name, bool condition)
    {
        if (condition) { passed++; Console.WriteLine("PASS " + name); }
        else { failed++; Console.WriteLine("FAIL " + name); }
    }
    private static object Field(object target, string name) => target.GetType().GetField(name, Private).GetValue(target);
    private static void Set(object target, string name, object value) => target.GetType().GetField(name, Private).SetValue(target, value);
    private static object Call(object target, string name, params object[] args) => target.GetType().GetMethod(name, Private).Invoke(target, args);
    private static void FileOfSize(string path, int bytes)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using (var file = File.Create(path)) file.SetLength(bytes);
    }
    private sealed class FailingCleaner : ICleaner
    {
        public (string FileType, int SpaceInMB) GetReclaimableSpace() => ("fixture", 0);
        public Task Reclaim() => throw new IOException("fixture failure");
    }
    private static async Task Run(RichTextBox log)
    {
        string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        string common = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        Check("Office resolves current user", (string)Field(new MicrosoftOfficeCacheCleaner(log), "_msoCacheDir") == Path.Combine(local, "Microsoft", "Office", "16.0", "OfficeFileCache"));
        Check("Edge resolves current user", ((string[])Field(new MicrosoftEdgeCacheCleaner(log), "_edgeCacheDirectories")).All(p => p.StartsWith(Path.Combine(local, "Microsoft", "Edge", "User Data"), StringComparison.OrdinalIgnoreCase) && !p.Contains("%")));
        Check("Installer Windows root", (string)Field(new WindowsInstallerCacheCleaner(log), "_windowsDirectory") == windows);
        Check("Defender ProgramData root", (string)Field(new WindowsDefenderLogFilesCleaner(log), "_programDataDirectory") == common);
        var cases = new[] {
            Tuple.Create(typeof(TemporaryFilesCleaner), "_tempDirectory"),
            Tuple.Create(typeof(TemporarySetupFilesCleaner), "_tempSetupDirectory"),
            Tuple.Create(typeof(TemporaryInternetFilesCleaner), "_tempInternetFilesDirectory"),
            Tuple.Create(typeof(DeliveryOptimizationFilesCleaner), "_deliveryOptimizationFilesDirectory"),
            Tuple.Create(typeof(WindowsErrorReportsCleaner), "_windowsErrorReportsDirectory"),
            Tuple.Create(typeof(MicrosoftOfficeCacheCleaner), "_msoCacheDir"),
            Tuple.Create(typeof(ChromeCacheCleaner), "_chromeCacheDirectories"),
            Tuple.Create(typeof(WindowsInstallerCacheCleaner), "_windowsDirectory"),
            Tuple.Create(typeof(WindowsDefenderLogFilesCleaner), "_logFilePaths"),
            Tuple.Create(typeof(WindowsUpdateLogsCleaner), "_windowsUpdateLogDir")
        };
        foreach (var entry in cases)
        {
            string name = entry.Item1.Name;
            string fixture = Path.Combine(Root, name);
            string target = name == "WindowsInstallerCacheCleaner" ? Path.Combine(fixture, "Installer", "$PatchCache$", "Managed") : fixture;
            FileOfSize(Path.Combine(target, "a.log"), 1024 * 1024);
            FileOfSize(Path.Combine(target, "nested", "b.log"), 2 * 1024 * 1024);
            FileOfSize(Path.Combine(Root, name + "-outside.txt"), 17);
            var cleaner = (ICleaner)Activator.CreateInstance(entry.Item1, log);
            Set(cleaner, entry.Item2, entry.Item2 == "_chromeCacheDirectories" || entry.Item2 == "_logFilePaths" ? (object)new[] { fixture } : fixture);
            Check(name + " estimate", cleaner.GetReclaimableSpace().SpaceInMB == 3);
            try {
                await cleaner.Reclaim();
                Check(name + " deletes fixture", !File.Exists(Path.Combine(target, "a.log")) && !File.Exists(Path.Combine(target, "nested", "b.log")));
                Check(name + " remaining space", cleaner.GetReclaimableSpace().SpaceInMB == 0);
                await cleaner.Reclaim();
                Check(name + " retry", cleaner.GetReclaimableSpace().SpaceInMB == 0);
            } catch (Exception ex) { Check(name + " execution " + ex.GetType().Name, false); }
            Check(name + " preserves outside file", File.Exists(Path.Combine(Root, name + "-outside.txt")));
        }
        foreach (var entry in cases.Take(5))
        {
            string fixture = Path.Combine(Root, "locked-" + entry.Item1.Name);
            string locked = Path.Combine(fixture, "a.lock");
            FileOfSize(locked, 128);
            FileOfSize(Path.Combine(fixture, "nested", "b.log"), 128);
            var cleaner = (ICleaner)Activator.CreateInstance(entry.Item1, log);
            Set(cleaner, entry.Item2, fixture);
            log.Clear();
            using (var handle = File.Open(locked, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                await cleaner.Reclaim();
            Check(entry.Item1.Name + " skips locked file and continues", File.Exists(locked) && !File.Exists(Path.Combine(fixture, "nested", "b.log")));
            Check(entry.Item1.Name + " reports failure in GUI", log.Text.Contains("无法") || log.Text.Contains("失败"));
        }
        string defender = Path.Combine(Root, "defender-selective");
        FileOfSize(Path.Combine(defender, "keep.db"), 128);
        FileOfSize(Path.Combine(defender, "remove.log"), 128);
        var defenderCleaner = new WindowsDefenderLogFilesCleaner(log);
        Set(defenderCleaner, "_logFilePaths", new[] { defender });
        try { await defenderCleaner.Reclaim(); Check("Defender preserves non-log data", File.Exists(Path.Combine(defender, "keep.db")) && !File.Exists(Path.Combine(defender, "remove.log"))); }
        catch { Check("Defender preserves non-log data", false); }
        using (var form = new CleanSweep.CleanSweep())
        {
            Call(form, "Form1_Load", form, EventArgs.Empty);
            var button = form.Controls.Find("SweepItButton", true)[0];
            Call(form, "HandleCheckmarks", false);
            Check("No selection disables cleaning", !button.Enabled);
            Call(form, "HandleCheckmarks", true);
            Check("Select all selects 16", ((CheckBox[])Field(form, "_checkedArray")).Count(c => c.Checked) == 16 && button.Enabled);
            Call(form, "HandleCheckmarks", false);
            Check("Deselect all clears 16", ((CheckBox[])Field(form, "_checkedArray")).All(c => !c.Checked) && !button.Enabled);
            var output = (RichTextBox)form.Controls.Find("outputWindow", true)[0];
            output.Text = "中文日志选择复制";
            output.Select(2, 2);
            var previous = new DataObject();
            var clipboard = Clipboard.GetDataObject();
            if (clipboard != null)
                foreach (string format in clipboard.GetFormats(false))
                    previous.SetData(format, false, clipboard.GetData(format, false));
            try {
                Clipboard.SetDataObject("sentinel", true, 20, 100);
                Call(form, "ToolStripButton1_Click_1", form, EventArgs.Empty);
                Check("Copies selected log", Clipboard.GetText() == "日志");
                output.Select(0, 0);
                Call(form, "ToolStripButton1_Click_1", form, EventArgs.Empty);
                Check("Copies entire log", Clipboard.GetText() == output.Text);
            } finally { Clipboard.SetDataObject(previous, true, 20, 100); }
            Call(form, "ToolStripButton2_Click_2", form, EventArgs.Empty);
            Check("Clears log", output.Text.Length == 0);
            Call(form, "SetDarkMode");
            Check("Dark theme", form.BackColor == System.Drawing.Color.Black);
            Call(form, "SetLightMode");
            Check("Light theme", form.BackColor == System.Drawing.Color.White);
            ((CheckBox[])Field(form, "_checkedArray"))[0].Checked = true;
            Call(form, "SaveSettingsEventHandler", form, new FormClosingEventArgs(CloseReason.UserClosing, false));
            var settings = form.GetType().Assembly.GetType("CleanSweep.Properties.Settings");
            var defaults = settings.GetProperty("Default").GetValue(null);
            Check("Window close saves selections", (bool)settings.GetProperty("Box1Checked").GetValue(defaults));
            string batchPath = Path.Combine(Root, "batch");
            FileOfSize(Path.Combine(batchPath, "delete.tmp"), 1024 * 1024);
            var batchCleaner = new TemporaryFilesCleaner(output);
            Set(batchCleaner, "_tempDirectory", batchPath);
            await (Task)Call(form, "RunCleaners", (object)new ICleaner[] { new FailingCleaner(), batchCleaner });
            Check("Batch continues after failure", !File.Exists(Path.Combine(batchPath, "delete.tmp")));
            Check("Batch reports failure", output.Text.Contains("fixture failure"));
            Check("Batch restores controls", button.Enabled && ((CheckBox[])Field(form, "_checkedArray")).All(c => c.Enabled));
            Check("Batch reports actual reclaimed space", output.Text.Contains("已释放空间：1 MB"));
            Call(form, "HandleCheckmarks", false);
            Call(form, "SaveUserSelectedChecks");
            ((System.Configuration.ApplicationSettingsBase)defaults).Save();
            ((CheckBox[])Field(form, "_checkedArray"))[0].Checked = true;
            var windowHandle = form.Handle;
            Call(form, "ExitToolStripMenuItem_Click", form, EventArgs.Empty);
            Check("Exit menu closes form", form.IsDisposed);
            Check("Exit menu saves selections", (bool)settings.GetProperty("Box1Checked").GetValue(defaults));
            settings.GetProperty("Box1Checked").SetValue(defaults, false);
            ((System.Configuration.ApplicationSettingsBase)defaults).Save();
        }
    }
}
