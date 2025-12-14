# Build MSI Installer for Windows Timer Lock
# This script should be run on a Windows machine with WiX Toolset installed

param(
    [string]$WixPath = "C:\Program Files (x86)\WiX Toolset v3.11\bin"
)

Write-Host "=== Building Windows Timer Lock MSI Installer ===" -ForegroundColor Cyan
Write-Host ""

# Check if WiX is installed
$candlePath = Join-Path $WixPath "candle.exe"
$lightPath = Join-Path $WixPath "light.exe"

if (-not (Test-Path $candlePath)) {
    Write-Host "ERROR: WiX Toolset not found at: $WixPath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install WiX Toolset v3.11 or later from:" -ForegroundColor Yellow
    Write-Host "https://github.com/wixtoolset/wix3/releases" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Or specify custom path:" -ForegroundColor Yellow
    Write-Host '.\build-msi.ps1 -WixPath "C:\Path\To\WiX\bin"' -ForegroundColor Cyan
    exit 1
}

Write-Host "Found WiX Toolset at: $WixPath" -ForegroundColor Green
Write-Host ""

# Check if executable exists
$exePath = "output\win-x64\WindowsTimerLock.exe"
if (-not (Test-Path $exePath)) {
    Write-Host "ERROR: Executable not found: $exePath" -ForegroundColor Red
    Write-Host "Please build the application first." -ForegroundColor Yellow
    exit 1
}

Write-Host "Found executable: $exePath" -ForegroundColor Green
Write-Host ""

# Create app icon if it doesn't exist (using default)
if (-not (Test-Path "app.ico")) {
    Write-Host "WARNING: app.ico not found, installer will proceed without custom icon" -ForegroundColor Yellow
    # Create a dummy file to prevent WiX error
    $null | Out-File "app.ico" -Encoding ASCII
}

# Create output directory
$outDir = "installer"
if (-not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir | Out-Null
}

# Step 1: Compile .wxs to .wixobj
Write-Host "[1/2] Compiling WiX source..." -ForegroundColor Yellow
& $candlePath -ext WixUtilExtension -arch x64 -out "$outDir\Installer.wixobj" Installer.wxs

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Compilation failed!" -ForegroundColor Red
    exit 1
}
Write-Host "      Compilation successful" -ForegroundColor Green
Write-Host ""

# Step 2: Link .wixobj to .msi
Write-Host "[2/2] Linking MSI..." -ForegroundColor Yellow
& $lightPath -ext WixUtilExtension -ext WixUIExtension -out "$outDir\WindowsTimerLock.msi" "$outDir\Installer.wixobj"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Linking failed!" -ForegroundColor Red
    exit 1
}
Write-Host "      MSI created successfully" -ForegroundColor Green
Write-Host ""

# Get MSI size
$msiFile = Get-Item "$outDir\WindowsTimerLock.msi"
$sizeMB = [math]::Round($msiFile.Length / 1MB, 2)

# Summary
Write-Host "=== Build Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "MSI Installer: $outDir\WindowsTimerLock.msi" -ForegroundColor Cyan
Write-Host "Size: $sizeMB MB" -ForegroundColor Cyan
Write-Host ""
Write-Host "To install:" -ForegroundColor Yellow
Write-Host "  1. Right-click the MSI file" -ForegroundColor White
Write-Host "  2. Select 'Install'" -ForegroundColor White
Write-Host "  3. Follow the installation wizard" -ForegroundColor White
Write-Host ""
Write-Host "The application will automatically:" -ForegroundColor Yellow
Write-Host "  - Install to C:\Program Files\WindowsTimerLock" -ForegroundColor White
Write-Host "  - Create a scheduled task to run at startup" -ForegroundColor White
Write-Host "  - Add Start Menu shortcut" -ForegroundColor White
Write-Host "  - Set proper permissions (users can't delete)" -ForegroundColor White
Write-Host ""
