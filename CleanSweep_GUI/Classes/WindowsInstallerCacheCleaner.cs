// Simplified Chinese localization by NiToot, 2026-11-11. See NOTICE.md.
using CleanSweep.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

public class WindowsInstallerCacheCleaner : ICleaner
{
    private readonly string _windowsDirectory;
    private long _preCleanupSize;
    private readonly RichTextBox _outputWindow;

    public WindowsInstallerCacheCleaner(RichTextBox outputWindow)
    {
        _windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        _outputWindow = outputWindow;
    }

    public (string FileType, int SpaceInMB) GetReclaimableSpace()
    {
        string patchCacheDir = Path.Combine(_windowsDirectory, "Installer", "$PatchCache$", "Managed");
        _preCleanupSize = CalculateDirectorySize(patchCacheDir);
        int spaceInMB = ConvertBytesToMegabytes(_preCleanupSize);
        return ("Windows Installer 缓存", spaceInMB);
    }

    public async Task Reclaim()
    {
        string patchCacheDir = Path.Combine(_windowsDirectory, "Installer", "$PatchCache$", "Managed");

        await Task.Run(() =>
        {
            try
            {
                if (Directory.Exists(patchCacheDir))
                {
                    Directory.Delete(patchCacheDir, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理 Windows Installer 缓存时出错：{ex.Message}");
            }
        });
        RichTextBoxExtensions.AppendText(_outputWindow, "已执行Windows Installer 缓存清理。\n");
    }

    public long ReportReclaimedSpace()
    {
        string patchCacheDir = Path.Combine(_windowsDirectory, "Installer", "$PatchCache$", "Managed");
        long postCleanupSize = CalculateDirectorySize(patchCacheDir);
        long reclaimedSpace = _preCleanupSize - postCleanupSize;
        return ConvertBytesToMegabytes(reclaimedSpace);
    }

    private long CalculateDirectorySize(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return 0;

        long totalSize = 0;
        try
        {
            foreach (var file in new DirectoryInfo(directoryPath).EnumerateFiles("*", SearchOption.AllDirectories))
            {
                try { totalSize += file.Length; }
                catch { }
            }
        }
        catch { }
        return totalSize;
    }

    private int ConvertBytesToMegabytes(long bytes)
    {
        return (int)Math.Min(bytes / 1024 / 1024, int.MaxValue);
    }
}
