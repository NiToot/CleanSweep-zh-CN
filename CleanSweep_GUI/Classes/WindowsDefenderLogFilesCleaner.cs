// Simplified Chinese localization by NiToot, 2026-11-11. See NOTICE.md.
using CleanSweep.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

public class WindowsDefenderLogFilesCleaner : ICleaner
{
    private readonly string _programDataDirectory;
    private long _preCleanupSize;
    private readonly string[] _logFilePaths;
    private readonly RichTextBox _outputWindow;

    public WindowsDefenderLogFilesCleaner(RichTextBox outputWindow)
    {
        _programDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        _logFilePaths = new[]
        {
            _programDataDirectory + @"\Microsoft\Windows Defender\Network Inspection System\Support\",
            _programDataDirectory + @"\Microsoft\Windows Defender\Scans\History\Service\",
            _programDataDirectory + @"\Microsoft\Windows Defender\Scans\History\ReportLatency\Latency",
            _programDataDirectory + @"\Microsoft\Windows Defender\Scans\History\Results\Resource",
            _programDataDirectory + @"\Microsoft\Windows Defender\Scans\History\Results\Quick",
            _programDataDirectory + @"\Microsoft\Windows Defender\Scans\History\CacheManager",
            _programDataDirectory + @"\Microsoft\Windows Defender\Scans\MetaStore",
            _programDataDirectory + @"\Microsoft\Windows Defender\Support"
        };
        _outputWindow = outputWindow;
    }

    public (string FileType, int SpaceInMB) GetReclaimableSpace()
    {
        _preCleanupSize = CalculateTotalLogFilesSize();
        int spaceInMB = ConvertBytesToMegabytes(_preCleanupSize);
        return ("Defender 日志", spaceInMB);
    }

    public Task Reclaim()
    {
        foreach (var directory in _logFilePaths)
            Helpers.CleanDirectoryContents(directory, _outputWindow, "*.log");
        RichTextBoxExtensions.AppendText(_outputWindow, "已处理 Defender 日志清理请求。\n");
        return Task.CompletedTask;
    }

    public long ReportReclaimedSpace()
    {
        long postCleanupSize = CalculateTotalLogFilesSize();
        long reclaimedSpace = _preCleanupSize - postCleanupSize;
        return ConvertBytesToMegabytes(reclaimedSpace);
    }

    private long CalculateTotalLogFilesSize()
    {
        long totalSize = 0;

        foreach (var path in _logFilePaths)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    foreach (var file in new DirectoryInfo(path).EnumerateFiles("*.log", SearchOption.AllDirectories))
                    {
                        try { totalSize += file.Length; }
                        catch { }
                    }
                }
                catch { }
            }
        }

        return totalSize;
    }

    private int ConvertBytesToMegabytes(long bytes)
    {
        return (int)Math.Min(bytes / 1024 / 1024, int.MaxValue);
    }
}
