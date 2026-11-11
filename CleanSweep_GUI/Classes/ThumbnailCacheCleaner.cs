// Simplified Chinese localization by NiToot, 2026-11-11. See NOTICE.md.
﻿using CleanSweep.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

public class ThumbnailCacheCleaner : ICleaner
{
    private readonly RichTextBox _outputWindow;

    public ThumbnailCacheCleaner(RichTextBox outputWindow)
    {
        _outputWindow = outputWindow;
    }

    public (string FileType, int SpaceInMB) GetReclaimableSpace()
    {
        return ("缩略图缓存", 0);
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
                        Arguments = "/C taskkill /f /im explorer.exe & timeout 1 & del /f /s /q /a %LocalAppData%\\Microsoft\\Windows\\Explorer\\thumbcache_*.db & timeout 1 & start %windir%\\explorer.exe",
                        UseShellExecute = true,
                        Verb = "runas",
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理缩略图缓存时出错：{ex.Message}");
            }
        });
        RichTextBoxExtensions.AppendText(_outputWindow, "已执行缩略图缓存清理。\n");
    }
}
