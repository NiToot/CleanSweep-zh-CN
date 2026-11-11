# 来源与修改说明

本仓库是 [Thomas Loupe / CleanSweep](https://github.com/thomasloupe/CleanSweep) 的非官方简体中文分支。GUI 汉化由 NiToot 于 2026-11-11 整理，基于提交 c8922a41900e658f7f50c1399c0b9ae1c954daa8（上游 v3.1）。

GUI 中文版按 GNU GPL 第 3 版发布，完整许可见 [CleanSweep_GUI/LICENSE](CleanSweep_GUI/LICENSE)。保留上游作者署名和许可文件。你可以依据该许可使用、修改及再发布 GUI 中文版；再发布修改版时须保留许可、注明修改并履行对应源码提供义务。本软件按现状提供，不提供任何担保。第三方组件按其各自许可提供。

此次修改涵盖 GUI 的菜单、16 个清理项、空间统计、日志、提示及对话框。清理目标和算法沿用上述上游提交。各修改文件附有汉化者和日期说明。

本次发布范围为 GUI。上游 CLI 文件保留在 Fork 中供追溯；当前上游只有 GUI 目录附有 LICENSE，CLI 的独立发布授权范围尚待确认，本仓库 Release 仅包含 GUI 运行文件。

GUI 使用 Octokit 13.0.1，版权归 GitHub, Inc.，适用 MIT 许可，全文见 THIRD-PARTY-NOTICES/Octokit-LICENSE.txt。

运行包随附对应版本的源码压缩包。构建需要 Windows、Visual Studio 或 Build Tools 的 .NET 桌面构建工具及 .NET Framework 4.8 目标包。在开发者命令行运行以下命令。

    msbuild CleanSweep_GUI\CleanSweep.csproj /p:Configuration=Release
