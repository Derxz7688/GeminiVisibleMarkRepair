Gemini 去水印单文件便携版 / Gemini Visible Mark Repair Portable - v1.0

用途 / Purpose:
本工具用于在本机修复 Gemini / Omni 视频画面中可见的角标区域，也就是常说的“可见水印”。它不检测、不移除、不修改 SynthID，只处理画面里能看见的角标。

This tool locally repairs the visible Gemini / Omni corner mark in videos you are authorized to edit. It does not detect, remove, or modify SynthID.

v1.0 实测范围 / Validation:
已实测横屏 Gemini 可见角标视频。Veo 视频、竖屏视频、非标准角标位置和复杂遮挡场景尚未完成实测，请视为实验性兼容范围。

Validated on landscape Gemini visible-mark videos. Veo videos, portrait videos, non-standard mark positions, and heavily occluded scenes are experimental only in v1.0.

当前成品 / Package:
1. GeminiVisibleMarkRepair.exe
   单文件启动版。双击即可使用，首次运行会把 FFmpeg、ffprobe、修复引擎和界面程序释放到本机 AppData 缓存目录。
   Portable single-file launcher. On first run it extracts FFmpeg, ffprobe, the repair engine, and the desktop UI to the local AppData cache.

2. GeminiVisibleMarkRepair_Source.zip
   干净源码包。包含核心脚本、桌面界面源码、自解压启动器源码、图标文件和项目描述，不包含旧日志、测试输出、处理视频和临时任务目录。
   Clean source archive. It does not include logs, test outputs, processed videos, or temporary job directories.

3. app-icon.ico / app-icon.png
   当前应用图标源文件。
   Icon source files.

使用方式 / Usage:
双击 GeminiVisibleMarkRepair.exe，拖入视频，点击开始修复。输出视频默认保存在单 EXE 同目录下的“输出”文件夹中。

Run GeminiVisibleMarkRepair.exe, drag in a video, then start repair. Outputs are saved to the "输出" folder next to the EXE by default.

注意 / Notes:
请只处理你有权编辑的视频。本工具不提供 SynthID 绕过功能。

Use this tool only on videos you are authorized to edit. This tool does not provide SynthID bypass functionality.
