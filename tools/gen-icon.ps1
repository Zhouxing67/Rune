# Generate app and Store icons from assets/branding/rune-pdf-icon.png.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationCore, PresentationFramework, WindowsBase
$root = Join-Path $PSScriptRoot '..'
$assets = Join-Path $root 'src\Rune.App\Assets'
$source = New-Object System.Windows.Media.Imaging.BitmapImage
$source.BeginInit()
$source.UriSource = [Uri](Join-Path (Resolve-Path $root).Path 'assets\branding\rune-pdf-icon.png')
$source.CacheOption = [System.Windows.Media.Imaging.BitmapCacheOption]::OnLoad
$source.EndInit()
$source.Freeze()
function Convert-IconBitmap([int]$Size) {
    $visual = New-Object System.Windows.Media.DrawingVisual
    $ctx = $visual.RenderOpen()
    $ctx.DrawImage($source, (New-Object System.Windows.Rect 0, 0, $Size, $Size))
    $ctx.Close()
    $bitmap = New-Object System.Windows.Media.Imaging.RenderTargetBitmap $Size, $Size, 96, 96, ([System.Windows.Media.PixelFormats]::Pbgra32)
    $bitmap.Render($visual)
    return $bitmap
}
function Save-Bitmap {
    param($Bitmap, [string]$Path)
    $encoder = New-Object System.Windows.Media.Imaging.PngBitmapEncoder
    $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($Bitmap))
    $stream = [System.IO.File]::Create($Path)
    $encoder.Save($stream)
    $stream.Dispose()
    Write-Host "wrote $(Split-Path -Leaf $Path)"
}

# Square art centred on a wider canvas (wide tile, splash screen).
function Save-Centred {
    param([int]$CanvasW, [int]$CanvasH, [int]$Art, [string]$Path)
    $visual = New-Object System.Windows.Media.DrawingVisual
    $ctx = $visual.RenderOpen()
    $ctx.DrawImage((Convert-IconBitmap $Art),
        (New-Object System.Windows.Rect ([double](($CanvasW - $Art) / 2)), ([double](($CanvasH - $Art) / 2)), ([double]$Art), ([double]$Art)))
    $ctx.Close()
    $rtb = New-Object System.Windows.Media.Imaging.RenderTargetBitmap(
        $CanvasW, $CanvasH, 96, 96, [System.Windows.Media.PixelFormats]::Pbgra32)
    $rtb.Render($visual)
    Save-Bitmap $rtb $Path
}

# ---- MSIX visual assets ----------------------------------------------------
# The previous set was scale-200 only, which is the bare minimum the Store
# accepts; Windows picks the nearest and rescales, which is why the taskbar
# icon looked soft. These are the scales Windows actually asks for.
$square = @(
    @{ Name = 'Square44x44Logo.scale-100.png'; Size = 44 },
    @{ Name = 'Square44x44Logo.scale-125.png'; Size = 55 },
    @{ Name = 'Square44x44Logo.scale-150.png'; Size = 66 },
    @{ Name = 'Square44x44Logo.scale-200.png'; Size = 88 },
    @{ Name = 'Square44x44Logo.scale-400.png'; Size = 176 },
    @{ Name = 'Square44x44Logo.targetsize-16.png'; Size = 16 },
    @{ Name = 'Square44x44Logo.targetsize-24.png'; Size = 24 },
    @{ Name = 'Square44x44Logo.targetsize-32.png'; Size = 32 },
    @{ Name = 'Square44x44Logo.targetsize-48.png'; Size = 48 },
    @{ Name = 'Square44x44Logo.targetsize-256.png'; Size = 256 },
    @{ Name = 'Square44x44Logo.targetsize-24_altform-unplated.png'; Size = 24 },
    @{ Name = 'Square44x44Logo.targetsize-48_altform-unplated.png'; Size = 48 },
    @{ Name = 'Square44x44Logo.targetsize-256_altform-unplated.png'; Size = 256 },
    @{ Name = 'Square150x150Logo.scale-100.png'; Size = 150 },
    @{ Name = 'Square150x150Logo.scale-125.png'; Size = 188 },
    @{ Name = 'Square150x150Logo.scale-150.png'; Size = 225 },
    @{ Name = 'Square150x150Logo.scale-200.png'; Size = 300 },
    @{ Name = 'Square150x150Logo.scale-400.png'; Size = 600 },
    @{ Name = 'StoreLogo.png'; Size = 50 },
    @{ Name = 'StoreLogo.scale-200.png'; Size = 100 }
)
foreach ($spec in $square) {
    Save-Bitmap (Convert-IconBitmap $spec.Size) (Join-Path $assets $spec.Name)
}

Save-Centred 310 150 150 (Join-Path $assets 'Wide310x150Logo.scale-100.png')
Save-Centred 620 300 300 (Join-Path $assets 'Wide310x150Logo.scale-200.png')
Save-Centred 620 300 300 (Join-Path $assets 'SplashScreen.scale-100.png')
Save-Centred 1240 600 600 (Join-Path $assets 'SplashScreen.scale-200.png')

# ---- Multi-size .ico (PNG-compressed entries) ------------------------------
$sizes = 16, 24, 32, 48, 64, 256
$pngs = foreach ($s in $sizes) {
    $encoder = New-Object System.Windows.Media.Imaging.PngBitmapEncoder
    $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create((Convert-IconBitmap $s)))
    $ms = New-Object System.IO.MemoryStream
    $encoder.Save($ms)
    , $ms.ToArray()
}

$icoPath = Join-Path $assets 'RunePdf.ico'
$stream = [System.IO.File]::Create($icoPath)
$writer = New-Object System.IO.BinaryWriter($stream)
$writer.Write([uint16]0)               # reserved
$writer.Write([uint16]1)               # type: icon
$writer.Write([uint16]$sizes.Count)    # image count
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $s = $sizes[$i]
    $writer.Write([byte]($s -band 0xFF))   # width (0 = 256)
    $writer.Write([byte]($s -band 0xFF))   # height
    $writer.Write([byte]0)                 # palette
    $writer.Write([byte]0)                 # reserved
    $writer.Write([uint16]1)               # planes
    $writer.Write([uint16]32)              # bpp
    $writer.Write([uint32]$pngs[$i].Length)
    $writer.Write([uint32]$offset)
    $offset += $pngs[$i].Length
}
foreach ($png in $pngs) { $writer.Write($png) }
$writer.Dispose(); $stream.Dispose()
Write-Host "wrote RunePdf.ico ($($sizes -join '/'))"
