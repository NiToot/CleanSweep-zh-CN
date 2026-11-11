// Simplified Chinese localization by NiToot, 2026-11-11. See NOTICE.md.
﻿using CleanSweep.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

public class WindowsOldDirectoryCleaner : ICleaner
{
    private readonly string _windowsOldDirectoryPath = "C:\\Windows.old";
    private long _preCleanupSize;
    private readonly RichTextBox _outputWindow;

    public WindowsOldDirectoryCleaner(RichTextBox outputWindow)
    {
        _outputWindow = outputWindow;
    }

    public (string FileType, int SpaceInMB) GetReclaimableSpace()
    {
        _preCleanupSize = CalculateDirectorySize(_windowsOldDirectoryPath);
        int spaceInMB = ConvertBytesToMegabytes(_preCleanupSize);
        return ("Windows.old 文件夹", spaceInMB);
    }

    public async Task Reclaim()
    {
        if (Directory.Exists(_windowsOldDirectoryPath))
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
                            Arguments = "/C takeown /F C:\\Windows.old* /R /A /D Y & cacls C:\\Windows.old*.* /T /grant administrators:F & rmdir /S /Q C:\\Windows.old",
                            UseShellExecute = true,
                            Verb = "runas",
                            WorkingDirectory = "C:\\Windows\\",
                            WindowStyle = ProcessWindowStyle.Hidden
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"无法删除 Windows.old 文件夹：{ex.Message}");
                }
            });
        }
        else
        {
            Console.WriteLine("未找到 Windows.old 文件夹。");
        }
        RichTextBoxExtensions.AppendText(_outputWindow, "已处理 Windows.old 文件夹清理请求。\n");
    }

    public long ReportReclaimedSpace()
    {
        long postCleanupSize = CalculateDirectorySize(_windowsOldDirectoryPath);
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
