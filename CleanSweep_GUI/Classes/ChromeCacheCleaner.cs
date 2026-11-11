// Simplified Chinese localization by NiToot, 2026-11-11. See NOTICE.md.
﻿using CleanSweep.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

public class ChromeCacheCleaner : ICleaner
{
    private readonly string[] _chromeCacheDirectories;
    private long _preCleanupSize;
    private readonly RichTextBox _outputWindow;

    public ChromeCacheCleaner(RichTextBox outputWindow)
    {
        _outputWindow = outputWindow;

        try
        {
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data");

            // Check if the base path exists before proceeding
            if (!Directory.Exists(basePath))
            {
                RichTextBoxExtensions.AppendText(_outputWindow, $"未找到 Chrome 用户数据目录：{basePath}。\n");
                return; // Exit the constructor early if base path doesn't exist
            }

            var directories = new DirectoryInfo(basePath).GetDirectories()
                .Where(dir => dir.Name.StartsWith("Profile ") || dir.Name == "Default")
                .Select(dir => Path.Combine(dir.FullName, "Cache"))
                .ToList();

            if (directories.Count == 0)
            {
                // If no directories found, add default cache path
                directories.Add(Path.Combine(basePath, "Default", "Cache"));
            }

            _chromeCacheDirectories = directories.ToArray();
        }
        catch (Exception ex)
        {
            RichTextBoxExtensions.AppendText(_outputWindow, $"初始化 Chrome 缓存清理时出错：{ex.Message}\n");
            _chromeCacheDirectories = Array.Empty<string>(); // Initialize as empty array to prevent further errors
        }
    }

    public (string FileType, int SpaceInMB) GetReclaimableSpace()
    {
        if (_chromeCacheDirectories == null || _chromeCacheDirectories.Length == 0)
        {
            return ("Chrome 缓存", 0); // Return 0 space if no directories are found
        }

        _preCleanupSize = _chromeCacheDirectories.Sum(CalculateDirectorySize);
        int spaceInMB = ConvertBytesToMegabytes(_preCleanupSize);
        return ("Chrome 缓存", spaceInMB);
    }

    public async Task Reclaim()
    {
        if (_chromeCacheDirectories == null || _chromeCacheDirectories.Length == 0)
        {
            RichTextBoxExtensions.AppendText(_outputWindow, "未找到可清理的 Chrome 缓存目录。\n");
            return; // Exit if no directories are found
        }

        await Task.Run(() =>
        {
            foreach (var chromeDirectory in _chromeCacheDirectories)
            {
                if (Directory.Exists(chromeDirectory))
                {
                    try
                    {
                        Directory.Delete(chromeDirectory, true);
                        RichTextBoxExtensions.AppendText(_outputWindow, $"已删除 Chrome 缓存目录：{chromeDirectory}\n");
                    }
                    catch (Exception ex)
                    {
                        RichTextBoxExtensions.AppendText(_outputWindow, $"无法删除 Chrome 缓存目录 {chromeDirectory}：{ex.Message}\n");
                    }
                }
                else
                {
                    RichTextBoxExtensions.AppendText(_outputWindow, $"未找到 Chrome 缓存目录：{chromeDirectory}\n");
                }
            }
        });

        RichTextBoxExtensions.AppendText(_outputWindow, "已执行Chrome 缓存清理。\n");
    }

    private long CalculateDirectorySize(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return 0;

        try
        {
            return new DirectoryInfo(directoryPath)
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Sum(file => file.Length);
        }
        catch (Exception ex)
        {
            RichTextBoxExtensions.AppendText(_outputWindow, $"无法计算目录 {directoryPath} 的大小：{ex.Message}\n");
            return 0;
        }
    }

    private int ConvertBytesToMegabytes(long bytes)
    {
        return (int)(bytes / 1024 / 1024);
    }
}
