# CleanSweep 简体中文版

基于 [Thomas Loupe / CleanSweep](https://github.com/thomasloupe/CleanSweep) v3.1 的非官方 Windows 清理工具 GUI 汉化版，由 [NiToot](https://github.com/NiToot) 维护。

[下载中文版](https://github.com/NiToot/CleanSweep-zh-CN/releases/latest) · [测试记录](tests/TEST-RESULTS.md) · [来源与修改说明](NOTICE.md) · [GPLv3 许可](CleanSweep_GUI/LICENSE)

## 使用方法

1. 下载 Release 中的运行包并完整解压。
2. 双击 CleanSweep.exe，按系统提示授予管理员权限。
3. 查看各类文件的预计可释放空间，勾选需要清理的项目。
4. 点击 **开始清理**，查看日志中的处理结果和错误信息。

程序需要 Windows 和 .NET Framework 4.8。本次测试在 Windows x64 环境完成，其他系统版本尚未逐一验证。

**选项 → 主题** 可切换深色和浅色。日志区支持复制选中文字、复制全部和清空日志；窗口关闭时保存勾选项及主题设置。

## 清理项目

- 临时文件、安装临时文件、Internet 临时文件。
- Chrome、Edge、Office 缓存，Windows Installer 补丁缓存。
- Windows 错误报告、更新日志、更新下载目录（上游称为“传递优化文件”）。
- Defender 日志、事件查看器日志、回收站、缩略图缓存、文件历史记录和 Windows.old。

清空回收站、移除 Windows.old、清理文件历史记录和事件日志会删除对应数据，请确认保留需求后勾选。部分清理操作会结束浏览器或重启资源管理器。Installer 缓存可能用于软件修复或卸载，清理前请确认能够重新取得安装介质。

## 本次修复

- 修正 Office、Edge 当前用户缓存路径，以及 Installer 和 Defender 的重复目录拼接。
- 修正后台清理线程写入日志时的跨线程异常。
- 文件被占用或访问失败时，在日志中显示原因，并继续处理其他可访问文件。
- 修正更新日志子目录清理遗漏；Defender 清理仅处理 .log 文件，保留其他数据。
- 修正复制选中文字、关闭窗口和退出菜单时的设置保存。
- 单项清理异常后继续处理其他选中项目，并恢复按钮状态。

## 测试范围

v3.1-zh-CN.2 发布前已完成 **80 项自动化检查，全部通过**。覆盖范围和限制如下。

测试使用真实 Release 程序集，将指定清理器的目录字段替换为本次创建的临时测试目录，实际创建并删除文件。覆盖 10 类文件清理器的空间估算、文件及子目录删除、重复执行和非目标文件保留，另有文件占用、中文日志、批量失败恢复及界面操作检查。

测试目录注入只存在于测试程序。界面批量执行测试还覆盖了一项失败后继续处理后续真实测试文件的行为。具体数量和结果见 [测试记录](tests/TEST-RESULTS.md)。

**系统级操作仍有验证缺口。** 回收站、事件日志、缩略图缓存、文件历史记录、Windows.old 和 Edge 清理的完整执行过程尚未在可丢弃 Windows 虚拟机中验证。隔离文件测试也不能证明真实系统目录中的权限、服务占用和软件修复行为。本次按已完成的测试范围发布，系统级验证暂未继续。

空间按整 MB 统计，小于 1 MB 的文件可能显示为 0 MB。部分上游清理器的异常处理和系统命令返回状态还需系统级验证。上游版本标签 v3,1 与程序内 v3.1 格式不同，检查上游更新可能重复提示。

## 构建与复测

安装 Visual Studio 或 Build Tools 的 .NET 桌面构建工具及 .NET Framework 4.8 目标包，在开发者 PowerShell 中运行。

    msbuild CleanSweep_GUI/CleanSweep.csproj /p:Configuration=Release
    msbuild tests/FunctionalTests.csproj /p:Configuration=Release
    ./tests/bin/Release/CleanSweep.FunctionalTests.exe

测试结束输出通过和失败数量，失败时返回非零退出码。测试会操作剪贴板并尝试恢复原内容，运行期间请避免同时复制其他内容。

## 许可与致谢

原项目由 Thomas Loupe 开发。GUI 中文版按 GPLv3 发布，保留原作者署名和许可，发布运行包附有对应源码。Octokit 使用 MIT 许可，全文见 [第三方许可](THIRD-PARTY-NOTICES/Octokit-LICENSE.txt)。

本仓库 Release 范围为 GUI。CLI 沿用 Fork 的上游内容，其独立发布授权范围尚待确认。
