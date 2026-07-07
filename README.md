# Gemini 去水印单文件便携版 / Gemini Visible Mark Repair Portable

> 中文：本工具用于在本机修复视频画面中可见的 Gemini / Omni 角标，也就是常说的“可见水印”。它不检测、不移除、不修改 Google SynthID 或其他不可见来源标识。
>
> English: This local Windows tool repairs the visible Gemini / Omni corner mark in videos you are authorized to edit. It does not detect, remove, or modify Google SynthID or any other invisible provenance watermark.

## 下载 / Download

从 GitHub Releases 下载单文件便携版：

Download the portable single-file Windows build from GitHub Releases:

- [GeminiVisibleMarkRepair_Final_v1.0.zip](https://github.com/Derxz7688/GeminiVisibleMarkRepair/releases/download/v1.0.0/GeminiVisibleMarkRepair_Final_v1.0.zip)
- 解压后双击 `GeminiVisibleMarkRepair.exe`
- First run extracts the bundled runtime to `%LOCALAPPDATA%\GeminiVisibleMarkRepair`
- 输出视频默认保存到 EXE 同目录下的 `输出` 文件夹

校验值见 [docs/CHECKSUMS-v1.0.txt](docs/CHECKSUMS-v1.0.txt)。

## 参考视频 / Reference Videos

| 文件 / File | 说明 / Description | 参数 / Specs |
| --- | --- | --- |
| [input.mp4](https://github.com/Derxz7688/GeminiVisibleMarkRepair/releases/download/v1.0.0/reference-input-gemini-watermark.mp4) | 原始参考视频，含横屏 Gemini 可见角标 / Source sample with visible Gemini corner mark | 1280x720, 24 fps, 10.0s, H.264 + AAC |
| [output.mp4](https://github.com/Derxz7688/GeminiVisibleMarkRepair/releases/download/v1.0.0/reference-output-gemini-repaired.mp4) | 修复后参考输出 / Repaired reference output | 1280x720, 24 fps, 10.0s, H.264 + AAC |

## v1.0 验证范围 / Validation Scope

已实测 / Validated:

- 横屏 Gemini 可见角标视频。
- Landscape Gemini visible-mark videos.
- 超过上游预览引擎帧数限制的视频自动分段、修复、拼接。
- Automatic segmenting, repairing, and stitching when a video exceeds the preview engine frame limit.
- 输出保留原始分辨率、帧率、时长、帧数和音轨。
- Output keeps the original resolution, frame rate, duration, frame count, and audio track.

未实测或不承诺 / Not validated or not guaranteed:

- Veo 样例视频。
- Veo sample videos.
- 竖屏视频、非标准角标位置、明显遮挡角标、复杂明暗变化场景。
- Portrait videos, non-standard mark positions, heavily occluded marks, or difficult lighting/background changes.
- 不可见 SynthID 或任何其他不可见来源标识。
- Invisible SynthID or any other invisible provenance watermark.

说明：底层运行时来自 `VeoWatermarkRemover v0.6.2-demo`，因此可能覆盖更多上游支持的角标形态；但本项目 v1.0 只按上述已实测范围承诺。

Note: the bundled runtime is based on `VeoWatermarkRemover v0.6.2-demo`, so additional upstream-supported mark shapes may work, but v1.0 only claims the validation scope listed above.

## 源码结构 / Source Layout

- `repair-video.ps1`: 核心处理流水线 / processing pipeline.
- `launcher/GeminiRepairApp.cs`: WinForms 桌面界面 / desktop UI.
- `packaging/SingleExeLauncher.cs`: 单文件自解压启动器 / self-extracting launcher.
- `public/` and `app-server.js`: 早期本地网页界面，保留作参考 / earlier local web UI kept for reference.
- `assets/`: 应用图标源文件 / icon sources.
- `THIRD_PARTY.md`: 第三方组件与来源说明 / third-party component notes.

源码仓库不包含 FFmpeg 二进制、上游预览修复引擎、测试视频、处理结果、日志和临时任务目录。

The source repository does not commit FFmpeg binaries, the upstream preview repair engine, test videos, processed outputs, logs, or temporary job directories.

## PowerShell 用法 / PowerShell Usage

如果你从源码恢复完整运行时，也可以直接调用脚本：

If you restore the full runtime from source, the script can be called directly:

```powershell
.\repair-video.ps1 "C:\path\input.mp4"
.\repair-video.ps1 "C:\path\input.mp4" "C:\path\output.mp4"
```

## 已知限制 / Known Limitations

- v1.0 内置的上游视频引擎是 unsigned preview binary；首次运行可能触发 Windows SmartScreen。
- The bundled upstream video engine is an unsigned preview binary; Windows SmartScreen may warn on first run.
- 上游引擎内部会重新编码视频，复杂运动背景、极亮/极暗背景仍可能出现局部修复痕迹。
- The upstream engine re-encodes video internally; difficult backgrounds may still show local repair artifacts.
- Veo 视频尚未在本项目 v1.0 中完成实测，请视为实验性兼容范围。
- Veo videos are not validated in v1.0 and should be treated as experimental compatibility only.
- 本项目仅面向用户有权处理的视频素材。
- Use this tool only on videos you are authorized to edit.
- 本项目不提供 SynthID 绕过功能。
- This project does not provide SynthID bypass functionality.

## 参考与来源 / References

- [allenk/VeoWatermarkRemover](https://github.com/allenk/VeoWatermarkRemover)
- [VeoWatermarkRemover v0.6.2-demo release](https://github.com/allenk/VeoWatermarkRemover/releases/tag/v0.6.2-demo)
- [FFmpeg](https://ffmpeg.org/)
