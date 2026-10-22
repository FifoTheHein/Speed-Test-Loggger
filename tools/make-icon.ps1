# Draws the app icon (a rising speed line on a blue tile) and writes:
#   SpeedTest\AppIcon.ico  - every Windows icon size, used for the exe, ClickOnce shortcut, windows and tray
#   docs\images\icon.png   - 128 px, for the README
#   deploy\icon.png        - 64 px, the install page's favicon
# Run from the repository root:  powershell -ExecutionPolicy Bypass -File tools\make-icon.ps1
# Then copy the .ico into the window resources with -UpdateResx (see the end of this script).
param([switch]$UpdateResx)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent

$accent = [System.Drawing.Color]::FromArgb(0, 103, 192)
$accentLight = [System.Drawing.Color]::FromArgb(76, 160, 236)

function New-RoundedRect([float]$x, [float]$y, [float]$w, [float]$h, [float]$r) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $r * 2
    $path.AddArc($x, $y, $d, $d, 180, 90)
    $path.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $path.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $path.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $path.CloseFigure()
    return $path
}

# Draws the icon at one size. Below 32 px the baseline is dropped and the line is thicker, so it stays crisp.
function New-IconBitmap([int]$s) {
    $bmp = New-Object System.Drawing.Bitmap $s, $s, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $g.PixelOffsetMode = 'HighQuality'

    $margin = [Math]::Max(0.5, $s * 0.06)
    $tile = New-RoundedRect $margin $margin ($s - 2 * $margin) ($s - 2 * $margin) ($s * 0.2)
    $fill = New-Object System.Drawing.Drawing2D.LinearGradientBrush((New-Object System.Drawing.PointF 0, 0), (New-Object System.Drawing.PointF $s, $s), $accentLight, $accent)
    $g.FillPath($fill, $tile)

    $small = $s -le 24
    $width = if ($small) { [Math]::Max(1.7, $s * 0.12) } else { $s * 0.08 }
    $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::White), $width
    $pen.StartCap = 'Round'; $pen.EndCap = 'Round'; $pen.LineJoin = 'Round'

    $top = if ($small) { 0.32 } else { 0.30 }
    $points = [System.Drawing.PointF[]]@(
        (New-Object System.Drawing.PointF ($s * 0.22), ($s * ($(if ($small) { 0.70 } else { 0.66 })))),
        (New-Object System.Drawing.PointF ($s * 0.40), ($s * 0.46)),
        (New-Object System.Drawing.PointF ($s * 0.55), ($s * 0.58)),
        (New-Object System.Drawing.PointF ($s * 0.76), ($s * $top)))
    $g.DrawLines($pen, $points)

    if (-not $small) {
        $baseline = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(140, 255, 255, 255)), ($s * 0.04)
        $baseline.StartCap = 'Round'; $baseline.EndCap = 'Round'
        $g.DrawLine($baseline, $s * 0.22, $s * 0.78, $s * 0.78, $s * 0.78)
    }

    $dot = if ($small) { [Math]::Max(3, $s * 0.2) } else { $s * 0.13 }
    $g.FillEllipse([System.Drawing.Brushes]::White, $s * 0.76 - $dot / 2, $s * $top - $dot / 2, $dot, $dot)

    $g.Dispose()
    return $bmp
}

# One icon frame: PNG for 256 px, a 32-bit DIB with an empty AND mask for the rest (what every Windows version reads)
function Get-FrameBytes([System.Drawing.Bitmap]$bmp) {
    $s = $bmp.Width
    $ms = New-Object System.IO.MemoryStream
    if ($s -ge 256) {
        $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        return $ms.ToArray()
    }
    $w = New-Object System.IO.BinaryWriter $ms
    # BITMAPINFOHEADER: the height covers the colour rows plus the AND mask rows
    $w.Write([int]40); $w.Write([int]$s); $w.Write([int]($s * 2)); $w.Write([int16]1); $w.Write([int16]32)
    $w.Write([int]0); $w.Write([int]0); $w.Write([int]0); $w.Write([int]0); $w.Write([int]0); $w.Write([int]0)
    # Colour rows, bottom-up, BGRA
    for ($y = $s - 1; $y -ge 0; $y--) {
        for ($x = 0; $x -lt $s; $x++) {
            $c = $bmp.GetPixel($x, $y)
            $w.Write([byte]$c.B); $w.Write([byte]$c.G); $w.Write([byte]$c.R); $w.Write([byte]$c.A)
        }
    }
    # AND mask: all zero (transparency comes from the alpha channel), each row padded to 4 bytes
    $maskRow = [int]([Math]::Ceiling($s / 32.0) * 4)
    $w.Write((New-Object byte[] ($maskRow * $s)))
    $w.Flush()
    return $ms.ToArray()
}

# No 128 px frame: uncompressed it's 64 KB, and Windows scales the 256 px PNG down for that size instead
$sizes = 16, 20, 24, 32, 40, 48, 64, 256
$frames = foreach ($s in $sizes) { , (Get-FrameBytes (New-IconBitmap $s)) }

$ico = New-Object System.IO.MemoryStream
$w = New-Object System.IO.BinaryWriter $ico
$w.Write([int16]0); $w.Write([int16]1); $w.Write([int16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $s = $sizes[$i]
    $w.Write([byte]($s % 256)); $w.Write([byte]($s % 256)); $w.Write([byte]0); $w.Write([byte]0)
    $w.Write([int16]1); $w.Write([int16]32); $w.Write([int]$frames[$i].Length); $w.Write([int]$offset)
    $offset += $frames[$i].Length
}
foreach ($f in $frames) { $w.Write([byte[]]$f) }
$w.Flush()

$icoPath = Join-Path $root 'SpeedTest\AppIcon.ico'
[System.IO.File]::WriteAllBytes($icoPath, $ico.ToArray())
New-Item -ItemType Directory -Force (Join-Path $root 'docs\images') | Out-Null
(New-IconBitmap 128).Save((Join-Path $root 'docs\images\icon.png'), [System.Drawing.Imaging.ImageFormat]::Png)
(New-IconBitmap 64).Save((Join-Path $root 'deploy\icon.png'), [System.Drawing.Imaging.ImageFormat]::Png)
"wrote $icoPath ($($ico.Length) bytes, sizes $($sizes -join ', ')), docs\images\icon.png, deploy\icon.png"

# The windows and tray icon are stored in the .resx files as the .ico file's bytes in base64
if ($UpdateResx) {
    $base64 = [Convert]::ToBase64String($ico.ToArray())
    $lines = for ($i = 0; $i -lt $base64.Length; $i += 76) { '        ' + $base64.Substring($i, [Math]::Min(76, $base64.Length - $i)) }
    $value = "<value>`r`n" + ($lines -join "`r`n") + "`r`n</value>"
    foreach ($file in 'SpeedTest\FormMain.resx', 'SpeedTest\FormSettings.resx') {
        $path = Join-Path $root $file
        $text = [System.IO.File]::ReadAllText($path)
        $pattern = '(<data name="(?:\$this|IconMain)\.Icon" type="System\.Drawing\.Icon, System\.Drawing" mimetype="application/x-microsoft\.net\.object\.bytearray\.base64">\s*)<value>[\s\S]*?</value>'
        $count = [regex]::Matches($text, $pattern).Count
        $text = [regex]::Replace($text, $pattern, { param($m) $m.Groups[1].Value + $value })
        [System.IO.File]::WriteAllText($path, $text, (New-Object System.Text.UTF8Encoding $true))
        "updated $count icon(s) in $file"
    }
}
