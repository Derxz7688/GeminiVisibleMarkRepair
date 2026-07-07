# Gemini / Omni Visible Mark Repair Tool

This is a local Windows desktop tool for repairing the visible Gemini / Omni /
Veo corner mark in videos that the user is authorized to edit.

It only works on the visible corner mark. It does not detect, modify, remove, or
interfere with invisible SynthID metadata or watermarking.

## Current Packaging

- `repair-video.ps1` is the processing pipeline.
- `launcher/GeminiRepairApp.cs` is the WinForms desktop UI.
- `packaging/SingleExeLauncher.cs` builds a self-extracting one-file launcher.
- The single EXE extracts the bundled runtime to `%LOCALAPPDATA%` on first run.
- Repaired videos are written to an `输出` folder next to the single EXE.

## Runtime Dependencies

The final single EXE bundles:

- FFmpeg / FFprobe
- The visible-mark repair engine
- The local PowerShell pipeline
- The WinForms UI

The source archive intentionally keeps source and packaging files clean and does
not include experimental logs, review outputs, processed videos, or old job
directories.
