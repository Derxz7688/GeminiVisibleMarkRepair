# Gemini Visible Mark Repair v1.0

## 下载

从 GitHub Release 下载：

- `GeminiVisibleMarkRepair_Final_20260707.zip`

解压后双击：

- `GeminiVisibleMarkRepair.exe`

## 适用范围

- 本机处理视频文件。
- 已实测：横屏 Gemini 可见角标视频。
- 实验性兼容：Veo / 竖屏 / 非标准位置角标，v1.0 未完成实测。
- 不检测、不修改、不移除 SynthID。

## v1.0 重点

- 单文件 Windows 桌面启动版。
- 内置 FFmpeg / FFprobe 和可见角标修复运行时。
- 支持超过预览引擎限制的视频自动分段、修复、拼接。
- 输出保留原始分辨率、帧率、时长、帧数和音轨。
- 生成 `.provenance.json` sidecar，记录处理说明。

## 校验

见 `docs/CHECKSUMS-v1.0.txt`。

## 注意

请只处理你有权编辑的视频。上游修复引擎为 unsigned preview binary，首次运行时 Windows 可能弹出 SmartScreen 安全提示。
