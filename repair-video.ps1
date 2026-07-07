param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$InputPath,

    [Parameter(Position = 1)]
    [string]$OutputPath,

    [switch]$NoTipCleanup,
    [switch]$KeepIntermediate
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$engine = Join-Path $root 'tools\GeminiWatermarkTool-Video.exe'
$ffmpeg = Join-Path $root 'tools\ffmpeg\bin\ffmpeg.exe'
$ffprobe = Join-Path $root 'tools\ffmpeg\bin\ffprobe.exe'
$maxSegmentSeconds = 9.0
$engineSafeSeconds = 10.0
$engineFrameLimit = 240
$targetSegmentFrames = 210

foreach ($required in @($engine, $ffmpeg, $ffprobe)) {
    if (-not (Test-Path -LiteralPath $required)) {
        throw "Missing required tool: $required"
    }
}

function Convert-ToDoubleInvariant {
    param([object]$Value)
    if ($null -eq $Value) { return 0.0 }
    return [double]::Parse([string]$Value, [System.Globalization.CultureInfo]::InvariantCulture)
}

function Format-Seconds {
    param([double]$Value)
    return $Value.ToString('0.###', [System.Globalization.CultureInfo]::InvariantCulture)
}

function Convert-FrameRateToDouble {
    param([string]$Value)
    if ([string]::IsNullOrWhiteSpace($Value) -or $Value -eq '0/0') { return 0.0 }
    if ($Value.Contains('/')) {
        $parts = $Value.Split('/')
        $num = Convert-ToDoubleInvariant $parts[0]
        $den = Convert-ToDoubleInvariant $parts[1]
        if ($den -ne 0) { return $num / $den }
        return 0.0
    }
    return Convert-ToDoubleInvariant $Value
}

function Get-MediaInfo {
    param([string]$Path, [switch]$CountFrames)
    $args = @('-v', 'error')
    if ($CountFrames) { $args += '-count_frames' }
    $args += @(
        '-show_entries',
        'format=duration:stream=index,codec_type,codec_name,width,height,r_frame_rate,avg_frame_rate,duration,nb_frames,nb_read_frames',
        '-of', 'json', '--', $Path
    )
    $json = & $ffprobe @args
    if ($LASTEXITCODE -ne 0) {
        throw "ffprobe failed for: $Path"
    }
    return ($json | Out-String | ConvertFrom-Json)
}

function Get-FirstStream {
    param([object]$Media, [string]$Type)
    return @($Media.streams) | Where-Object { $_.codec_type -eq $Type } | Select-Object -First 1
}

function Get-VideoDuration {
    param([object]$Media)
    $video = Get-FirstStream $Media 'video'
    if ($video -and $video.duration) { return Convert-ToDoubleInvariant $video.duration }
    if ($Media.format -and $Media.format.duration) { return Convert-ToDoubleInvariant $Media.format.duration }
    throw 'Unable to determine input video duration.'
}

function Get-FrameRateText {
    param([object]$Media)
    $video = Get-FirstStream $Media 'video'
    if ($video -and $video.r_frame_rate -and $video.r_frame_rate -ne '0/0') {
        return [string]$video.r_frame_rate
    }
    if ($video -and $video.avg_frame_rate -and $video.avg_frame_rate -ne '0/0') {
        return [string]$video.avg_frame_rate
    }
    return $null
}

function Get-CountedFrames {
    param([object]$Media)
    $video = Get-FirstStream $Media 'video'
    if ($video -and $video.nb_read_frames) { return [int64]$video.nb_read_frames }
    if ($video -and $video.nb_frames) { return [int64]$video.nb_frames }
    return 0
}

function ConvertTo-ConcatPath {
    param([string]$Path)
    return (($Path -replace '\\', '/') -replace "'", "\\'")
}

function Invoke-VisibleMarkRepairSegment {
    param(
        [string]$SegmentInput,
        [string]$SegmentOutput,
        [string]$Label
    )

    $segmentWork = Join-Path $workDir "repair-$Label"
    New-Item -ItemType Directory -Force -Path $segmentWork | Out-Null
    $engineOutput = Join-Path $segmentWork 'engine-output.mp4'

    Write-Host "Running visible-mark repair engine for $Label..."
    $previousErrorPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    $engineLog = & $engine --no-banner --veo --verbose -o $engineOutput $SegmentInput 2>&1
    $engineExit = $LASTEXITCODE
    $ErrorActionPreference = $previousErrorPreference
    $engineLog | ForEach-Object { Write-Host $_ }
    if ($engineExit -ne 0 -or -not (Test-Path -LiteralPath $engineOutput)) {
        throw "Repair engine failed for $Label with exit code $engineExit."
    }

    $logText = $engineLog -join "`n"
    $match = [regex]::Match($logText, '-> \((?<x>\d+),(?<y>\d+)\) (?<w>\d+)x(?<h>\d+)')
    $usedTipCleanup = $false
    $repairedLeadingFrames = 0
    $filterParts = @()
    $ffmpegInputs = @('-i', $engineOutput)

    # The preview engine can hard-skip low-confidence opening frames. Repair only
    # a short, consecutive leading gap; later skips may be intentional occlusions.
    if ($match.Success) {
        $processedFrames = [regex]::Matches($logText, 'frame (?<n>\d+):') |
            ForEach-Object { [int]$_.Groups['n'].Value } |
            Sort-Object -Unique
        $firstProcessed = if ($processedFrames.Count -gt 0) { $processedFrames[0] } else { 0 }
        if ($firstProcessed -gt 0 -and $firstProcessed -le 72) {
            $x = [int]$match.Groups['x'].Value
            $y = [int]$match.Groups['y'].Value
            $w = [int]$match.Groups['w'].Value
            $h = [int]$match.Groups['h'].Value
            Write-Host "Repairing $firstProcessed low-confidence opening frames for $Label..."

            for ($frame = 0; $frame -lt $firstProcessed; $frame++) {
                $sourceFrame = Join-Path $segmentWork "opening-$frame.png"
                $fixedFrame = Join-Path $segmentWork "opening-$frame-fixed.png"
                & $ffmpeg -hide_banner -loglevel error -y -i $SegmentInput -vf "select=eq(n\,$frame)" -vsync 0 -frames:v 1 $sourceFrame
                if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $sourceFrame)) {
                    throw "Failed to extract opening frame $frame for $Label."
                }

                $previousErrorPreference = $ErrorActionPreference
                $ErrorActionPreference = 'Continue'
                & $engine --no-banner --force --region "$x,$y,$w,$h" --veo-alpha 0.45 `
                    --denoise ai --strength 180 -o $fixedFrame $sourceFrame 2>&1 | Out-Null
                $imageExit = $LASTEXITCODE
                $ErrorActionPreference = $previousErrorPreference
                if ($imageExit -ne 0 -or -not (Test-Path -LiteralPath $fixedFrame)) {
                    throw "Failed to repair opening frame $frame for $Label."
                }

                $ffmpegInputs += @('-i', $fixedFrame)
                $inputLabel = if ($frame -eq 0) { '[0:v]' } else { "[lead$frame]" }
                $outputLabel = "[lead$($frame + 1)]"
                $filterParts += "$inputLabel[$($frame + 1):v]overlay=enable='eq(n\,$frame)'$outputLabel"
            }
            $repairedLeadingFrames = $firstProcessed
        }
    }

    if (-not $NoTipCleanup -and $match.Success -and
        [int]$match.Groups['w'].Value -eq 45 -and [int]$match.Groups['h'].Value -eq 45) {
        $x = [int]$match.Groups['x'].Value
        $y = [int]$match.Groups['y'].Value
        $cx = $x + 22
        $cy = $y + 22
        $tipFilter = @(
            "delogo=x=$($cx-3):y=$($y-3):w=7:h=7:show=0"
            "delogo=x=$($x-2):y=$($cy-2):w=5:h=5:show=0"
            "delogo=x=$($x+42):y=$($cy-2):w=5:h=5:show=0"
            "delogo=x=$($cx-2):y=$($y+42):w=5:h=5:show=0"
        ) -join ','

        Write-Host "Applying 45x45 Omni tip cleanup for $Label..."
        $baseLabel = if ($repairedLeadingFrames -gt 0) { "[lead$repairedLeadingFrames]" } else { '[0:v]' }
        $filterParts += "$baseLabel$tipFilter[vout]"
        $usedTipCleanup = $true
    } elseif ($repairedLeadingFrames -gt 0) {
        $filterParts += "[lead$repairedLeadingFrames]null[vout]"
    }

    if ($filterParts.Count -gt 0) {
        $filter = $filterParts -join ';'
        & $ffmpeg -hide_banner -loglevel error -y @ffmpegInputs -filter_complex $filter `
            -map '[vout]' -map '0:a?' -c:v libx264 -crf 14 -preset slow -pix_fmt yuv420p `
            -c:a copy -movflags +faststart $SegmentOutput
        if ($LASTEXITCODE -ne 0) {
            throw "FFmpeg finalization failed for $Label with exit code $LASTEXITCODE."
        }
    } else {
        Move-Item -LiteralPath $engineOutput -Destination $SegmentOutput -Force
    }

    $region = $null
    if ($match.Success) {
        $region = [ordered]@{
            x = [int]$match.Groups['x'].Value
            y = [int]$match.Groups['y'].Value
            w = [int]$match.Groups['w'].Value
            h = [int]$match.Groups['h'].Value
        }
    }

    return [ordered]@{
        label = $Label
        output = $SegmentOutput
        region = $region
        tipCleanup = $usedTipCleanup
        repairedLeadingFrames = $repairedLeadingFrames
    }
}

function Join-VideoWithOriginalAudio {
    param(
        [string]$VideoPath,
        [string]$OriginalPath,
        [string]$FinalPath,
        [object]$OriginalInfo,
        [int64]$FrameLimit
    )

    $videoForMux = $VideoPath
    if ($FrameLimit -gt 0) {
        $videoForMux = Join-Path $workDir 'final-video-frame-trimmed.mp4'
        & $ffmpeg -hide_banner -loglevel error -y -i $VideoPath `
            -map '0:v:0' -frames:v ([string]$FrameLimit) -an -c:v copy -movflags +faststart $videoForMux
        if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $videoForMux)) {
            throw "Failed to trim final video to $FrameLimit frames."
        }
    }

    $audio = Get-FirstStream $OriginalInfo 'audio'
    if ($audio) {
        & $ffmpeg -hide_banner -loglevel error -y -i $videoForMux -i $OriginalPath `
            -map '0:v:0' -map '1:a?' -c:v copy -c:a copy -movflags +faststart $FinalPath
    } else {
        & $ffmpeg -hide_banner -loglevel error -y -i $videoForMux `
            -map '0:v:0' -c:v copy -movflags +faststart $FinalPath
    }
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $FinalPath)) {
        throw "Failed to write final output: $FinalPath"
    }
}

function Assert-OutputMatchesInput {
    param([string]$OriginalPath, [string]$FinalPath)
    $inputCounted = Get-MediaInfo $OriginalPath -CountFrames
    $outputCounted = Get-MediaInfo $FinalPath -CountFrames
    $inputVideo = Get-FirstStream $inputCounted 'video'
    $outputVideo = Get-FirstStream $outputCounted 'video'
    if ($inputVideo.width -ne $outputVideo.width -or $inputVideo.height -ne $outputVideo.height) {
        throw "Output resolution mismatch: input $($inputVideo.width)x$($inputVideo.height), output $($outputVideo.width)x$($outputVideo.height)."
    }
    $inputFrames = Get-CountedFrames $inputCounted
    $outputFrames = Get-CountedFrames $outputCounted
    if ($inputFrames -gt 0 -and $outputFrames -gt 0 -and [math]::Abs($inputFrames - $outputFrames) -gt 1) {
        throw "Output frame count mismatch: input $inputFrames, output $outputFrames."
    }
    $inputDuration = Get-VideoDuration $inputCounted
    $outputDuration = Get-VideoDuration $outputCounted
    if ([math]::Abs($inputDuration - $outputDuration) -gt 0.25) {
        throw "Output video duration mismatch: input $(Format-Seconds $inputDuration)s, output $(Format-Seconds $outputDuration)s."
    }
    return [ordered]@{
        inputFrames = $inputFrames
        outputFrames = $outputFrames
        inputVideoDuration = $inputDuration
        outputVideoDuration = $outputDuration
        outputMedia = $outputCounted
    }
}

$input = (Resolve-Path -LiteralPath $InputPath).Path
$inputItem = Get-Item -LiteralPath $input
if (-not $OutputPath) {
    $OutputPath = Join-Path $inputItem.DirectoryName "$($inputItem.BaseName)_visible_mark_repaired.mp4"
}
$output = [System.IO.Path]::GetFullPath($OutputPath)
if ($output -eq $input) {
    throw 'Output path must not overwrite the input video.'
}

$outputDir = Split-Path -Parent $output
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
$workDir = Join-Path $env:TEMP "gemini-visible-mark-repair-$([guid]::NewGuid())"
New-Item -ItemType Directory -Force -Path $workDir | Out-Null

try {
    $inputInfo = Get-MediaInfo $input
    $inputDuration = Get-VideoDuration $inputInfo
    $frameRateText = Get-FrameRateText $inputInfo
    $frameRate = Convert-FrameRateToDouble $frameRateText
    $inputFrameLimit = Get-CountedFrames $inputInfo
    if ($inputFrameLimit -le 0) {
        $inputFrameLimit = Get-CountedFrames (Get-MediaInfo $input -CountFrames)
    }
    $effectiveSegmentSeconds = $maxSegmentSeconds
    if ($frameRate -gt 0) {
        $frameBasedSeconds = $targetSegmentFrames / $frameRate
        $effectiveSegmentSeconds = [math]::Min($maxSegmentSeconds, $frameBasedSeconds)
        if ($effectiveSegmentSeconds -lt 1.0) { $effectiveSegmentSeconds = 1.0 }
    }
    $needsSegmentation = ($inputDuration -gt $engineSafeSeconds) -or ($inputFrameLimit -gt $engineFrameLimit)
    $segments = @()
    $repairedVideos = @()
    $stitchedVideo = Join-Path $workDir 'stitched-video.mp4'

    if ($needsSegmentation) {
        Write-Host "Input is $(Format-Seconds $inputDuration)s / $inputFrameLimit frames; demo engine is limited to about 10s or $engineFrameLimit frames."
        Write-Host "Splitting video into <= $(Format-Seconds $effectiveSegmentSeconds)s chunks for full-length repair..."
        $segmentPattern = Join-Path $workDir 'segment-%03d.mp4'
        $segmentArgs = @(
            '-hide_banner', '-loglevel', 'error', '-y',
            '-i', $input,
            '-map', '0:v:0',
            '-an',
            '-c:v', 'libx264',
            '-crf', '14',
            '-preset', 'veryfast',
            '-pix_fmt', 'yuv420p'
        )
        if ($frameRateText) {
            $segmentArgs += @('-r', $frameRateText)
        }
        $segmentArgs += @(
            '-force_key_frames', "expr:gte(t,n_forced*$effectiveSegmentSeconds)",
            '-f', 'segment',
            '-segment_time', (Format-Seconds $effectiveSegmentSeconds),
            '-reset_timestamps', '1',
            $segmentPattern
        )
        & $ffmpeg @segmentArgs
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to split input video."
        }

        $sourceSegments = @(Get-ChildItem -LiteralPath $workDir -Filter 'segment-*.mp4' | Sort-Object Name)
        if ($sourceSegments.Count -eq 0) {
            throw 'Video splitting produced no segments.'
        }

        for ($i = 0; $i -lt $sourceSegments.Count; $i++) {
            $label = "segment $($i + 1)/$($sourceSegments.Count)"
            Write-Host "Processing $label..."
            $segmentOutput = Join-Path $workDir ("repaired-{0:000}.mp4" -f $i)
            $detail = Invoke-VisibleMarkRepairSegment $sourceSegments[$i].FullName $segmentOutput $label
            $segments += $detail
            $repairedVideos += $segmentOutput
        }

        $concatList = Join-Path $workDir 'repaired-list.txt'
        $concatLines = $repairedVideos | ForEach-Object { "file '$(ConvertTo-ConcatPath $_)'" }
        Set-Content -LiteralPath $concatList -Encoding ascii -Value $concatLines
        & $ffmpeg -hide_banner -loglevel error -y -f concat -safe 0 -i $concatList -c copy $stitchedVideo
        if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $stitchedVideo)) {
            throw 'Failed to concatenate repaired video segments.'
        }
    } else {
        Write-Host "Input is $(Format-Seconds $inputDuration)s; processing as a single segment..."
        $singleVideo = Join-Path $workDir 'single-repaired-video.mp4'
        $detail = Invoke-VisibleMarkRepairSegment $input $singleVideo 'single segment'
        $segments += $detail
        Move-Item -LiteralPath $singleVideo -Destination $stitchedVideo -Force
    }

    Join-VideoWithOriginalAudio $stitchedVideo $input $output $inputInfo $inputFrameLimit
    $validation = Assert-OutputMatchesInput $input $output

    $sidecar = [ordered]@{
        source = $input
        output = $output
        processedAt = (Get-Date).ToString('o')
        engine = 'VeoWatermarkRemover v0.6.2-demo'
        visibleMarkOnly = $true
        synthIdModified = $false
        segmented = $needsSegmentation
        segmentSeconds = $effectiveSegmentSeconds
        engineFrameLimit = $engineFrameLimit
        targetSegmentFrames = $targetSegmentFrames
        segments = $segments
        validation = $validation
        media = $validation.outputMedia
        note = 'Google AI generated, visible mark repaired'
    }
    $sidecar | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath "$output.provenance.json" -Encoding utf8
    Write-Host "Done: $output"
    Write-Host "Validated: $($validation.outputFrames)/$($validation.inputFrames) video frames, $(Format-Seconds $($validation.outputVideoDuration))s video."
} finally {
    if ($KeepIntermediate) {
        Write-Host "Intermediate files kept at: $workDir"
    } elseif (Test-Path -LiteralPath $workDir) {
        Remove-Item -LiteralPath $workDir -Recurse -Force
    }
}
