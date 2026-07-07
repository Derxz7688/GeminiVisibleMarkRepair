# Gemini / Omni Visible Mark Repair

本项目是一个本地 Windows 桌面小工具，用于修复用户有权编辑的视频画面中可见的 Gemini / Omni 角标区域。

它只处理画面中肉眼可见的角标区域，不检测、不移除、不修改 Google SynthID 或其他不可见来源标识。

## v1.0 成品

v1.0 的可直接运行版本请从 GitHub Releases 下载：

- `GeminiVisibleMarkRepair_Final_20260707.zip`
- 解压后双击 `GeminiVisibleMarkRepair.exe`
- 首次运行会释放内置运行时到本机 `%LOCALAPPDATA%\GeminiVisibleMarkRepair`
- 输出视频默认保存到 EXE 同目录下的 `输出` 文件夹

校验值见 [docs/CHECKSUMS-v1.0.txt](docs/CHECKSUMS-v1.0.txt)。

## 源码结构

- `repair-video.ps1`：核心处理流水线。
- `launcher/GeminiRepairApp.cs`：WinForms 桌面界面。
- `packaging/SingleExeLauncher.cs`：单文件自解压启动器。
- `public/` 与 `app-server.js`：早期本地网页界面，保留作参考。
- `assets/`：应用图标源文件。
- `THIRD_PARTY.md`：第三方组件与来源说明。

源码仓库不包含 FFmpeg 二进制、上游预览修复引擎、测试视频、处理结果、日志和临时任务目录。

## v1.0 验证范围

已实测：

- 横屏 Gemini 可见角标视频。
- 超过上游预览引擎帧数限制的视频自动分段、修复、拼接。
- 输出保留原始分辨率、帧率、时长、帧数和音轨。

未实测或不承诺：

- Veo 样例视频。
- 竖屏视频、非标准角标位置、明显遮挡角标、复杂明暗变化场景。
- 不可见 SynthID 或任何其他不可见来源标识。

说明：底层运行时来自 `VeoWatermarkRemover v0.6.2-demo`，因此可能覆盖更多上游支持的角标形态；但本项目 v1.0 只按上述已实测范围承诺。

## PowerShell 用法

如果你从源码恢复完整运行时，也可以直接调用脚本：

```powershell
.\repair-video.ps1 "C:\path\input.mp4"
.\repair-video.ps1 "C:\path\input.mp4" "C:\path\output.mp4"
```

处理流程：

1. 调用 `VeoWatermarkRemover v0.6.2-demo` 做多帧检测、反向 alpha 修复和边缘清理。
2. 处理超过引擎帧数上限的视频时，按帧率自动分段，修复后重新拼接。
3. 对特定 `45x45` Omni 角标额外修复四个残留尖角区域。
4. 保留原始宽高、帧率、时长、帧数和音轨。
5. 生成相邻 `.provenance.json`，记录处理来源和参数。

## 已知限制

- v1.0 内置的上游视频引擎是 unsigned preview binary；首次运行可能触发 Windows SmartScreen。
- 上游引擎内部会重新编码视频，复杂运动背景、极亮/极暗背景仍可能出现局部修复痕迹。
- Veo 视频尚未在本项目 v1.0 中完成实测，请视为实验性兼容范围。
- 本项目仅面向用户有权处理的视频素材。
- 本项目不提供 SynthID 绕过功能。

## 参考与来源

- [allenk/VeoWatermarkRemover](https://github.com/allenk/VeoWatermarkRemover)
- [VeoWatermarkRemover v0.6.2-demo release](https://github.com/allenk/VeoWatermarkRemover/releases/tag/v0.6.2-demo)
- [FFmpeg](https://ffmpeg.org/)
