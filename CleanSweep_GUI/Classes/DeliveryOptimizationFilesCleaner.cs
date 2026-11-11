// Simplified Chinese localization by NiToot, 2026-11-11. See NOTICE.md.
using CleanSweep.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

public class DeliveryOptimizationFilesCleaner : ICleaner
{
    private readonly string _deliveryOptimizationFilesDirectory;
    private long _deliveryOptimizationFilesDirSizeInMegaBytes;
    private readonly RichTextBox _outputWindow;

    public DeliveryOptimizationFilesCleaner(RichTextBox outputWindow)
    {
        _deliveryOptimizationFilesDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download");
        _outputWindow = outputWindow;
    }

    public (string FileType, int SpaceInMB) GetReclaimableSpace()
    {
        long sizeInBytes = CalculateDirectorySize(_deliveryOptimizationFilesDirectory);

        int sizeInMB = ConvertBytesToMegabytes(sizeInBytes);
        _deliveryOptimizationFilesDirSizeInMegaBytes = sizeInMB;
        return ("传递优化文件", sizeInMB);
    }

    public Task Reclaim()
    {
        Helpers.CleanDirectoryContents(_deliveryOptimizationFilesDirectory, _outputWindow);
        RichTextBoxExtensions.AppendText(_outputWindow, "已处理传递优化文件清理请求。\n");
        return Task.CompletedTask;
    }

    public long ReportReclaimedSpace()
    {
        long newSize = ConvertBytesToMegabytes(CalculateDirectorySize(_deliveryOptimizationFilesDirectory));
        return _deliveryOptimizationFilesDirSizeInMegaBytes - newSize;
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
        long sizeInMB = bytes / 1024 / 1024;
        return (int)Math.Min(sizeInMB, int.MaxValue);
    }
}
