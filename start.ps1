param([switch]$NoBrowser)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$url = 'http://127.0.0.1:4317/'

$existing = Get-NetTCPConnection -LocalPort 4317 -State Listen -ErrorAction SilentlyContinue
if ($existing) {
    Stop-Process -Id $existing.OwningProcess -Force
}

if (-not $NoBrowser) {
    Start-Process $url
}

Write-Host "Gemini visible mark repair tool: $url"
node (Join-Path $root 'app-server.js')
