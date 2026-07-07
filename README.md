<h1 align="center">Gemini 去水印单文件便携版</h1>

<p align="center">
  <strong>Gemini Visible Mark Repair Portable</strong><br>
  本地 Windows 单文件工具，用于修复视频画面中可见的 Gemini / Omni 角标。
</p>

<p align="center">
  <a href="https://github.com/Derxz7688/GeminiVisibleMarkRepair/releases/tag/v1.0.0"><img alt="release" src="https://img.shields.io/github/v/release/Derxz7688/GeminiVisibleMarkRepair?style=for-the-badge"></a>
  <a href="https://github.com/Derxz7688/GeminiVisibleMarkRepair/blob/main/LICENSE"><img alt="license" src="https://img.shields.io/github/license/Derxz7688/GeminiVisibleMarkRepair?style=for-the-badge"></a>
  <img alt="platform" src="https://img.shields.io/badge/platform-Windows-0078D4?style=for-the-badge&logo=windows">
  <img alt="scope" src="https://img.shields.io/badge/SynthID-not%20modified-6f42c1?style=for-the-badge">
</p>

<p align="center">
  <a href="https://derxz7688.github.io/GeminiVisibleMarkRepair/"><strong>在线展示页 / Live Demo Page</strong></a>
  ·
  <a href="https://github.com/Derxz7688/GeminiVisibleMarkRepair/releases/download/v1.0.0/GeminiVisibleMarkRepair_Final_v1.0.zip"><strong>下载单文件便携版 / Download</strong></a>
  ·
  <a href="https://github.com/Derxz7688/GeminiVisibleMarkRepair/releases/tag/v1.0.0">Release v1.0.0</a>
</p>

> 中文：本工具只修复画面中肉眼可见的 Gemini / Omni 角标，也就是常说的“可见水印”。它不检测、不移除、不修改 Google SynthID 或其他不可见来源标识。
>
> English: This tool only repairs the visible Gemini / Omni corner mark in videos you are authorized to edit. It does not detect, remove, or modify Google SynthID or any other invisible provenance watermark.

## 效果演示 / Demo

> 如果 GitHub README 内嵌视频没有自动播放，请打开 [在线展示页](https://derxz7688.github.io/GeminiVisibleMarkRepair/) 查看。
>
> If the embedded videos do not play inside the README, open the [live demo page](https://derxz7688.github.io/GeminiVisibleMarkRepair/).

### Before / 修复前

<video src="https://github.com/Derxz7688/GeminiVisibleMarkRepair/releases/download/v1.0.0/reference-input-gemini-watermark.mp4" controls width="100%"></video>

[直接打开修复前视频 / Open before video](https://github.com/Derxz7688/GeminiVisibleMarkRepair/releases/download/v1.0.0/reference-input-gemini-watermark.mp4)

### After / 修复后

<video src="https://github.com/Derxz7688/GeminiVisibleMarkRepair/releases/download/v1.0.0/reference-output-gemini-repaired.mp4" controls width="100%"></video>

[直接打开修复后视频 / Open after video](https://github.com/Derxz7688/GeminiVisibleMarkRepair/releases/download/v1.0.0/reference-output-gemini-repaired.mp4)

参考视频参数 / Reference video specs: `1280x720`, `24 fps`, `10.0s`, `H.264 + AAC`.

## 快速使用 / Quick Start

| 步骤 | 中文 | English |
| --- | --- | --- |
| 1 | 下载 `GeminiVisibleMarkRepair_Final_v1.0.zip` | Download `GeminiVisibleMarkRepair_Final_v1.0.zip` |
| 2 | 解压后双击 `GeminiVisibleMarkRepair.exe` | Extract and run `GeminiVisibleMarkRepair.exe` |
| 3 | 拖入视频，点击开始修复 | Drag in a video and start repair |
| 4 | 输出默认保存到 EXE 同目录下的 `输出` 文件夹 | Output is saved to the `输出` folder next to the EXE |

首次运行会释放内置运行时到 `%LOCALAPPDATA%\GeminiVisibleMarkRepair`。校验值见 [docs/CHECKSUMS-v1.0.txt](docs/CHECKSUMS-v1.0.txt)。

On first run, the launcher extracts the bundled runtime to `%LOCALAPPDATA%\GeminiVisibleMarkRepair`. Checksums are listed in [docs/CHECKSUMS-v1.0.txt](docs/CHECKSUMS-v1.0.txt).

## 功能亮点 / Highlights

| 能力 | 说明 |
| --- | --- |
| 单文件便携版 | 一个 EXE 启动桌面界面，适合不习惯 PowerShell 的用户。 |
| 本机处理 | 文件只在本机处理，不上传到外部服务。 |
| 保留视频参数 | 尽量保持原始分辨率、帧率、时长、帧数和音轨。 |
| 自动分段 | 对超过上游预览引擎帧数限制的视频自动分段、修复、拼接。 |
| 来源记录 | 输出相邻 `.provenance.json`，记录处理来源和参数。 |

| Feature | Description |
| --- | --- |
| Portable single EXE | Runs a desktop UI from one executable. |
| Local processing | Files are processed locally and are not uploaded. |
| Preserves media properties | Keeps resolution, frame rate, duration, frame count, and audio where possible. |
| Automatic segmenting | Splits, repairs, and stitches videos that exceed the preview engine frame limit. |
| Provenance sidecar | Writes an adjacent `.provenance.json` with processing notes. |

## v1.0 验证范围 / Validation Scope

已实测 / Validated:

- 横屏 Gemini 可见角标视频。
- Landscape Gemini visible-mark videos.
- 超过上游预览引擎帧数限制的视频自动分段、修复、拼接。
- Automatic segmenting, repairing, and stitching when a video exceeds the preview engine frame limit.

未实测或不承诺 / Not validated or not guaranteed:

- Veo 样例视频。
- Veo sample videos.
- 竖屏视频、非标准角标位置、明显遮挡角标、复杂明暗变化场景。
- Portrait videos, non-standard mark positions, heavily occluded marks, or difficult lighting/background changes.
- 不可见 SynthID 或任何其他不可见来源标识。
- Invisible SynthID or any other invisible provenance watermark.

## 源码结构 / Source Layout

- `repair-video.ps1`: 核心处理流水线 / processing pipeline.
- `launcher/GeminiRepairApp.cs`: WinForms 桌面界面 / desktop UI.
- `packaging/SingleExeLauncher.cs`: 单文件自解压启动器 / self-extracting launcher.
- `docs/index.html`: GitHub Pages 展示页 / live demo page.
- `THIRD_PARTY.md`: 第三方组件与来源说明 / third-party component notes.

源码仓库不包含 FFmpeg 二进制、上游预览修复引擎、测试视频、处理结果、日志和临时任务目录。

The source repository does not commit FFmpeg binaries, the upstream preview repair engine, test videos, processed outputs, logs, or temporary job directories.

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
