# Build Notes

This project currently has two user-facing forms:

- Portable folder build: `GeminiVisibleMarkRepair.exe` plus adjacent runtime files.
- Single EXE build: a self-extracting launcher with the same runtime files embedded.

The single EXE launcher appends a ZIP payload to the end of the launcher binary.
The footer format is:

1. ZIP payload bytes
2. 8-byte little-endian payload length
3. ASCII marker `GVMR_PAYLOAD_V1`

On first run, the launcher extracts the payload to:

`%LOCALAPPDATA%\GeminiVisibleMarkRepair\bundle-<payload-sha-prefix>`

The launcher then starts the inner WinForms app and sets `GVMR_OUTPUT_DIR` so
processed videos are written to an `输出` folder next to the single EXE.

The source archive does not include the FFmpeg binaries or the preview repair
engine. The final single EXE contains those runtime dependencies.
