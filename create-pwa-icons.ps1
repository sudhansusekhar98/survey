# PowerShell script to create PWA icons from existing logo
# This creates simple colored squares as placeholder icons

$iconSizes = @(72, 96, 128, 144, 152, 192, 384, 512)
$outputDir = "wwwroot\img\icons"
$sourceLogo = "wwwroot\img\icons\vlaccess_logo.png"

# Check if .NET Drawing libraries are available
Add-Type -AssemblyName System.Drawing

if (Test-Path $sourceLogo) {
    Write-Host "Creating PWA icons from $sourceLogo..." -ForegroundColor Green
    
    # Load source image
    $sourceImage = [System.Drawing.Image]::FromFile((Resolve-Path $sourceLogo))
    
    foreach ($size in $iconSizes) {
        $outputPath = Join-Path $outputDir "icon-${size}x${size}.png"
        
        # Create bitmap with gradient background
        $bitmap = New-Object System.Drawing.Bitmap($size, $size)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
        
        # Create gradient background
        $rect = New-Object System.Drawing.Rectangle(0, 0, $size, $size)
        $color1 = [System.Drawing.Color]::FromArgb(102, 126, 234) # #667eea
        $color2 = [System.Drawing.Color]::FromArgb(118, 75, 162)  # #764ba2
        $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, $color1, $color2, 135)
        $graphics.FillRectangle($brush, $rect)
        
        # Calculate logo size (80% of icon size)
        $logoSize = [int]($size * 0.8)
        $logoX = [int](($size - $logoSize) / 2)
        $logoY = [int](($size - $logoSize) / 2)
        $logoRect = New-Object System.Drawing.Rectangle($logoX, $logoY, $logoSize, $logoSize)
        
        # Draw logo centered
        $graphics.DrawImage($sourceImage, $logoRect)
        
        # Save icon
        $bitmap.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
        $graphics.Dispose()
        $bitmap.Dispose()
        $brush.Dispose()
        
        Write-Host "Created: icon-${size}x${size}.png" -ForegroundColor Cyan
    }
    
    $sourceImage.Dispose()
    Write-Host "`nAll PWA icons created successfully!" -ForegroundColor Green
    
} else {
    Write-Host "Source logo not found at: $sourceLogo" -ForegroundColor Red
    Write-Host "Creating simple gradient icons instead..." -ForegroundColor Yellow
    
    foreach ($size in $iconSizes) {
        $outputPath = Join-Path $outputDir "icon-${size}x${size}.png"
        
        # Create simple gradient icon
        $bitmap = New-Object System.Drawing.Bitmap($size, $size)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        
        $rect = New-Object System.Drawing.Rectangle(0, 0, $size, $size)
        $color1 = [System.Drawing.Color]::FromArgb(102, 126, 234)
        $color2 = [System.Drawing.Color]::FromArgb(118, 75, 162)
        $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, $color1, $color2, 135)
        $graphics.FillRectangle($brush, $rect)
        
        # Add text "S" in center
        $font = New-Object System.Drawing.Font("Arial", ($size * 0.5), [System.Drawing.FontStyle]::Bold)
        $stringFormat = New-Object System.Drawing.StringFormat
        $stringFormat.Alignment = [System.Drawing.StringAlignment]::Center
        $stringFormat.LineAlignment = [System.Drawing.StringAlignment]::Center
        $textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
        $graphics.DrawString("S", $font, $textBrush, $size/2, $size/2, $stringFormat)
        
        $bitmap.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
        $graphics.Dispose()
        $bitmap.Dispose()
        $brush.Dispose()
        $font.Dispose()
        $textBrush.Dispose()
        $stringFormat.Dispose()
        
        Write-Host "Created: icon-${size}x${size}.png" -ForegroundColor Cyan
    }
    
    Write-Host "`nAll PWA icons created successfully!" -ForegroundColor Green
}

Write-Host "`nYou can now install the PWA app!" -ForegroundColor Green
