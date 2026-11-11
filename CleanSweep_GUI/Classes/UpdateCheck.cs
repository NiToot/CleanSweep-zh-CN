// Simplified Chinese localization by NiToot, 2026-11-11. See NOTICE.md.
﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Octo = Octokit;

namespace CleanSweep.Classes
{
    internal class UpdateCheck
    {
        private static Octo.GitHubClient _octoClient;

        public static async Task CheckForUpdates()
        {
            if (_octoClient == null)
            {
                _octoClient = new Octo.GitHubClient(new Octo.ProductHeaderValue("CleanSweep"));
            }

            try
            {
                var version = CleanSweepVersion.Version;
                var releases = await _octoClient.Repository.Release.GetAll("thomasloupe", "CleanSweep").ConfigureAwait(false);
                Octo.Release latest = releases.OrderByDescending(r => r.PublishedAt).FirstOrDefault();

                if (latest == null || version == latest.TagName)
                {
                    MessageBox.Show($"当前版本对应上游最新版本 {version}。", "检查上游更新");
                    return;
                }

                if (version != latest.TagName)
                {
                    // Show message with a single link to get the latest version
                    CustomMessageBox.Show(
                        $"上游发布了 {latest.TagName}。\n此中文版基于 {version}。",
                        "发现上游新版本",
                        ("打开上游发布页面", $"https://github.com/thomasloupe/CleanSweep/releases/tag/{latest.TagName}")
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"检查上游更新失败：{ex.Message}", "错误");
            }
        }
    }
}
