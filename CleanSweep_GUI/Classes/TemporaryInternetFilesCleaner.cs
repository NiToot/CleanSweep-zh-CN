// Simplified Chinese localization by NiToot, 2026-11-11. See NOTICE.md.
using CleanSweep.Interfaces;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

public class TemporaryInternetFilesCleaner : ICleaner
{
    private readonly string _tempInternetFilesDirectory;
    private long _preCleanupSize;
    private readonly RichTextBox _outputWindow;

    public TemporaryInternetFilesCleaner(RichTextBox outputWindow)
    {
        _tempInternetFilesDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Microsoft", "Windows", "INetCache"
        );
        _outputWindow = outputWindow;
    }

    public (string FileType, int SpaceInMB) GetReclaimableSpace()
    {
        _preCleanupSize = CalculateDirectorySize(_tempInternetFilesDirectory);
        int totalSizeMBInt = ConvertBytesToMegabytes(_preCleanupSize);
        return ("Internet 临时文件", totalSizeMBInt);
    }

    public Task Reclaim()
    {
        Helpers.CleanDirectoryContents(_tempInternetFilesDirectory, _outputWindow);
        RichTextBoxExtensions.AppendText(_outputWindow, "已处理Internet 临时文件清理请求。\n");
        return Task.CompletedTask;
    }

    public long ReportReclaimedSpace()
    {
        long postCleanupSize = CalculateDirectorySize(_tempInternetFilesDirectory);
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
