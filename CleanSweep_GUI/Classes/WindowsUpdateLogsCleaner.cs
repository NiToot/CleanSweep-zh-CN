// Simplified Chinese localization by NiToot, 2026-11-11. See NOTICE.md.
using CleanSweep.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

public class WindowsUpdateLogsCleaner : ICleaner
{
    private readonly string _windowsUpdateLogDir;
    private long _preCleanupSize;
    private readonly RichTextBox _outputWindow;

    public WindowsUpdateLogsCleaner(RichTextBox outputWindow)
    {
        _windowsUpdateLogDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Logs", "WindowsUpdate");
        _outputWindow = outputWindow;
    }

    public (string FileType, int SpaceInMB) GetReclaimableSpace()
    {
        _preCleanupSize = CalculateDirectorySize(_windowsUpdateLogDir);
        int spaceInMB = ConvertBytesToMegabytes(_preCleanupSize);
        return ("Windows 更新日志", spaceInMB);
    }

    public Task Reclaim()
    {
        Helpers.CleanDirectoryContents(_windowsUpdateLogDir, _outputWindow);
        RichTextBoxExtensions.AppendText(_outputWindow, "已处理Windows 更新日志清理请求。\n");
        return Task.CompletedTask;
    }

    public long ReportReclaimedSpace()
    {
        long postCleanupSize = CalculateDirectorySize(_windowsUpdateLogDir);
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
