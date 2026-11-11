// Functional fixes by NiToot, 2026-11-11. See NOTICE.md.
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public static class Helpers
{
    public static void CleanDirectoryContents(string path, RichTextBox outputWindow, string pattern = "*")
    {
        if (!Directory.Exists(path))
            return;
        try
        {
            var directory = new DirectoryInfo(path);
            if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                RichTextBoxExtensions.AppendText(outputWindow, $"已跳过目录链接：{path}\n");
                return;
            }
            foreach (var file in directory.GetFiles(pattern))
            {
                try { file.Delete(); }
                catch (Exception ex)
                {
                    RichTextBoxExtensions.AppendText(outputWindow, $"无法删除文件 {file.FullName}：{ex.Message}\n");
                }
            }
            foreach (var child in directory.GetDirectories())
            {
                if ((child.Attributes & FileAttributes.ReparsePoint) != 0)
                    continue;
                CleanDirectoryContents(child.FullName, outputWindow, pattern);
                try
                {
                    if (child.GetFileSystemInfos().Length == 0)
                        child.Delete();
                }
                catch (Exception ex)
                {
                    RichTextBoxExtensions.AppendText(outputWindow, $"无法删除目录 {child.FullName}：{ex.Message}\n");
                }
            }
        }
        catch (Exception ex)
        {
            RichTextBoxExtensions.AppendText(outputWindow, $"无法读取目录 {path}：{ex.Message}\n");
        }
    }

    public static void AddWaitText(RichTextBox outputWindow)
    {
        if (outputWindow.InvokeRequired)
        {
            outputWindow.Invoke(new Action(() => AddWaitText(outputWindow)));
        }
        else
        {
            outputWindow.AppendText(".");
        }
    }
}

public static class RichTextBoxExtensions
{
    public static void AppendText(this RichTextBox box, string text)
    {
        if (box.IsDisposed)
            return;
        if (box.InvokeRequired)
        {
            box.Invoke(new Action(() => AppendText(box, text)));
            return;
        }
        box.HideSelection = false;
        box.SelectionStart = box.TextLength;
        box.SelectionLength = 0;
        box.AppendText(text);
        box.SelectionColor = box.ForeColor;
    }
}
