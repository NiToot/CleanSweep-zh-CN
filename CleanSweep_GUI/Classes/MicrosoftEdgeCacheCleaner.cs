// Simplified Chinese localization by NiToot, 2026-11-11. See NOTICE.md.
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CleanSweep.Interfaces;

public class MicrosoftEdgeCacheCleaner : ICleaner
{
    private readonly string[] _edgeCacheDirectories;
    private long _preCleanupSize;
    private readonly RichTextBox _outputWindow;

    public MicrosoftEdgeCacheCleaner(RichTextBox outputWindow)
    {
        _edgeCacheDirectories = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "User Data", "Default", "Cache"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "User Data", "Profile 1", "Cache")
        };
        _outputWindow = outputWindow;
    }

    public (string FileType, int SpaceInMB) GetReclaimableSpace()
    {
        _preCleanupSize = _edgeCacheDirectories.Sum(dir => CalculateDirectorySize(dir));
        int spaceInMB = ConvertBytesToMegabytes(_preCleanupSize);
        return ("Edge 缓存", spaceInMB);
    }

    public async Task Reclaim()
    {
        await Task.Run(() =>
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/C TASKKILL /F /IM msedge.exe",
                        UseShellExecute = true,
                        Verb = "runas",
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };
                process.Start();
                process.WaitForExit();

                foreach (var edgeDirectory in _edgeCacheDirectories)
                {
                    if (Directory.Exists(edgeDirectory))
                    {
                        try
                        {
                            Directory.Delete(edgeDirectory, true);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"无法删除 Edge 缓存目录 {edgeDirectory}：{ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理 Edge 缓存时出错：{ex.Message}");
            }
        });
        RichTextBoxExtensions.AppendText(_outputWindow, "已执行 Edge 缓存清理。\n");
    }

    public long ReportReclaimedSpace()
    {
        long postCleanupSize = _edgeCacheDirectories.Sum(dir => CalculateDirectorySize(dir));
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
