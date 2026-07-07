# Gemini 去水印单文件便携版 v1.0

[在线展示页 / Live Demo Page](https://derxz7688.github.io/GeminiVisibleMarkRepair/)

## 下载 / Download

- `GeminiVisibleMarkRepair_Final_v1.0.zip`
- 解压后双击 `GeminiVisibleMarkRepair.exe`
- Extract and run `GeminiVisibleMarkRepair.exe`

## 效果演示 / Demo

### Before / 修复前

<video src="https://github.com/Derxz7688/GeminiVisibleMarkRepair/releases/download/v1.0.0/reference-input-gemini-watermark.mp4" controls width="100%"></video>

[直接打开修复前视频 / Open before video](https://github.com/Derxz7688/GeminiVisibleMarkRepair/releases/download/v1.0.0/reference-input-gemini-watermark.mp4)

### After / 修复后

<video src="https://github.com/Derxz7688/GeminiVisibleMarkRepair/releases/download/v1.0.0/reference-output-gemini-repaired.mp4" controls width="100%"></video>

[直接打开修复后视频 / Open after video](https://github.com/Derxz7688/GeminiVisibleMarkRepair/releases/download/v1.0.0/reference-output-gemini-repaired.mp4)

Both videos are 1280x720, 24 fps, 10.0s, H.264 + AAC.

## 适用范围 / Scope

- 已实测：横屏 Gemini 可见角标视频。
- Validated: landscape Gemini visible-mark videos.
- 实验性兼容：Veo / 竖屏 / 非标准位置角标，v1.0 未完成实测。
- Experimental only: Veo, portrait layout, and non-standard mark positions.
- 不检测、不修改、不移除 SynthID。
- Does not detect, modify, or remove SynthID.

## v1.0 重点 / Highlights

- 单文件 Windows 桌面启动版。
- Portable single-file Windows desktop build.
- 内置 FFmpeg / FFprobe 和可见角标修复运行时。
- Bundles FFmpeg / FFprobe and the visible-mark repair runtime.
- 支持超过预览引擎限制的视频自动分段、修复、拼接。
- Automatically segments, repairs, and stitches videos that exceed the preview engine limit.
- 输出保留原始分辨率、帧率、时长、帧数和音轨。
- Keeps original resolution, frame rate, duration, frame count, and audio.
- 生成 `.provenance.json` sidecar，记录处理说明。
- Writes a `.provenance.json` sidecar.

## 注意 / Notes

请只处理你有权编辑的视频。上游修复引擎为 unsigned preview binary，首次运行时 Windows 可能弹出 SmartScreen 安全提示。

Use this tool only on videos you are authorized to edit. The upstream repair engine is an unsigned preview binary, so Windows SmartScreen may warn on first run.
