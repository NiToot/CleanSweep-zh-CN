# CleanSweep 简体中文版

基于 [Thomas Loupe / CleanSweep](https://github.com/thomasloupe/CleanSweep) v3.1 的非官方 Windows 图形界面汉化版，由 [NiToot](https://github.com/NiToot) 整理。

[下载中文版](https://github.com/NiToot/CleanSweep-zh-CN/releases/latest) · [修改与许可说明](NOTICE.md) · [GPLv3 许可](CleanSweep_GUI/LICENSE)

## 使用

解压运行包，双击 CleanSweep.exe，勾选清理项后点击 **开始清理**。需要 .NET Framework 4.8；程序会请求管理员权限。

菜单、16 个清理项、空间估算、日志和对话框均已汉化。可在 **选项 → 主题** 切换深色和浅色。系统和外部命令生成的消息使用系统语言。

清空回收站、删除 Windows.old、清理文件历史记录和事件日志会删除对应数据，请确认保留需求后勾选。部分清理操作会结束浏览器或重启资源管理器。

## 验证与已知限制

已完成 Release 构建、中文标签及布局检查、启动空间估算验证。实际删除操作尚未验证。

此版本沿用上游清理实现。上游更新检查可能因版本标签 v3,1 与程序内 v3.1 格式不同而重复提示更新；菜单链接指向原项目发布页。

## 源码与许可

GUI 中文版按 GPLv3 发布，保留原作者署名及许可，运行包附有对应源码。具体修改、构建方法和第三方许可见 [NOTICE.md](NOTICE.md)。

本次 Release 提供 GUI 中文版。CLI 文件沿用 Fork 的上游内容，其独立发布授权范围尚待确认。
