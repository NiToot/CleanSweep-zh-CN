// Simplified Chinese localization by NiToot, 2026-11-11. See NOTICE.md.
﻿using CleanSweep.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

public class EventViewerLogsCleaner : ICleaner
{
    private readonly RichTextBox _outputWindow;

    public EventViewerLogsCleaner(RichTextBox outputWindow)
    {
        _outputWindow = outputWindow;
    }

    public async Task Reclaim()
    {
        await Task.Run(async () =>
        {
            try
            {
                var commands = new[] { "cl Application", "cl Security", "cl System" };

                foreach (var command in commands)
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "wevtutil.exe",
                            Arguments = command,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        }
                    };

                    process.Start();

                    while (!process.HasExited)
                    {
                        Helpers.AddWaitText(_outputWindow);
                        await Task.Delay(1000);
                    }

                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理事件查看器日志时出错：{ex.Message}");
            }
        });
        RichTextBoxExtensions.AppendText(_outputWindow, "已执行事件查看器日志清理。\n");
    }

    (string FileType, int SpaceInMB) ICleaner.GetReclaimableSpace()
    {
        return ("事件查看器日志", 0);
    }
}
