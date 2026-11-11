// Simplified Chinese localization by NiToot, 2026-11-11. See NOTICE.md.
using CleanSweep.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

public class MicrosoftOfficeCacheCleaner : ICleaner
{
    private readonly string _msoCacheDir;
    private long _preCleanupSize;
    private readonly RichTextBox _outputWindow;

    public MicrosoftOfficeCacheCleaner(RichTextBox outputWindow)
    {
        _msoCacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Office", "16.0", "OfficeFileCache");
        _outputWindow = outputWindow;
    }

    public (string FileType, int SpaceInMB) GetReclaimableSpace()
    {
        _preCleanupSize = CalculateDirectorySize(_msoCacheDir);
        int spaceInMB = ConvertBytesToMegabytes(_preCleanupSize);
        return ("Office 缓存", spaceInMB);
    }

    public Task Reclaim()
    {
        try
        {
            if (Directory.Exists(_msoCacheDir))
            {
                try
                {
                    Directory.Delete(_msoCacheDir, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"无法删除 Office 缓存目录 {_msoCacheDir}：{ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"清理 Office 缓存时出错：{ex.Message}");
        }

        RichTextBoxExtensions.AppendText(_outputWindow, "已执行Office 缓存清理。\n");
        return Task.CompletedTask;
    }

    public long ReportReclaimedSpace()
    {
        long postCleanupSize = CalculateDirectorySize(_msoCacheDir);
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
        return (int)(bytes / 1024 / 1024);
    }
}
