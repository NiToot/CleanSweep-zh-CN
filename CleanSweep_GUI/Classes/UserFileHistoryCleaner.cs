// Simplified Chinese localization by NiToot, 2026-11-11. See NOTICE.md.
﻿using CleanSweep.Interfaces;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

public class UserFileHistoryCleaner : ICleaner
{
    private readonly RichTextBox _outputWindow;

    public UserFileHistoryCleaner(RichTextBox outputWindow)
    {
        _outputWindow = outputWindow;
    }

    public (string FileType, int SpaceInMB) GetReclaimableSpace()
    {
        return ("文件历史记录", 0);
    }

    public async Task Reclaim()
    {
        if (!IsFileHistoryEnabled())
        {
            Console.WriteLine("文件历史记录未启用，已跳过。");
            return;
        }

        try
        {
            await Task.Run(() =>
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/C FhManagew.exe -cleanup 0",
                        UseShellExecute = true,
                        Verb = "runas",
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };
                process.Start();
                process.WaitForExit();
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"清理文件历史记录时出错：{ex.Message}");
        }
        RichTextBoxExtensions.AppendText(_outputWindow, "已处理文件历史记录清理请求（如已启用）。\n");
    }

    private bool IsFileHistoryEnabled()
    {
        const string fileHistoryRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\FileHistory";

        try
        {
            object enabledValue = Registry.GetValue(fileHistoryRegistryKey, "Enabled", 0);
            return enabledValue != null && (int)enabledValue == 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"无法检查文件历史记录状态：{ex.Message}");
            return false;
        }
    }
}
