# Install Liquid Page Item Template
# This script copies the Liquid Page item template to Visual Studio's item templates directory

param(
    [string]$VSVersion = "2026"
)

$ErrorActionPreference = "Stop"

Write-Host "Installing Liquid Page Item Template for Visual Studio $VSVersion..." -ForegroundColor Cyan

# Determine Visual Studio version
$vsVersions = @{
    "2022" = "Visual Studio 2022"
    "2026" = "Visual Studio 2026"
    "17" = "Visual Studio 2022"
    "18" = "Visual Studio 2026"
}

$vsName = $vsVersions[$VSVersion]
if (-not $vsName) {
    Write-Host "Unsupported Visual Studio version: $VSVersion" -ForegroundColor Red
    Write-Host "Supported versions: 2022, 2026" -ForegroundColor Yellow
    exit 1
}

Write-Host "Target: $vsName" -ForegroundColor Green

# Paths
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourceDir = Join-Path $scriptDir "ItemTemplates\LiquidPage"
$targetBaseDir = Join-Path $env:USERPROFILE "Documents\$vsName\Templates\ItemTemplates\Visual C#"
$targetDir = Join-Path $targetBaseDir "LiquidPage"

Write-Host ""
Write-Host "Source: $sourceDir" -ForegroundColor Gray
Write-Host "Target: $targetDir" -ForegroundColor Gray
Write-Host ""

# Check if source exists
if (-not (Test-Path $sourceDir)) {
    Write-Host "Error: Source template directory not found!" -ForegroundColor Red
    Write-Host "Expected: $sourceDir" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Make sure you've built the extension project first." -ForegroundColor Yellow
    exit 1
}

# Create target directory if needed
if (-not (Test-Path $targetBaseDir)) {
    Write-Host "Creating templates directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $targetBaseDir -Force | Out-Null
}

# Remove existing template if present
if (Test-Path $targetDir) {
    Write-Host "Removing existing template..." -ForegroundColor Yellow
    Remove-Item $targetDir -Recurse -Force
}

# Copy template files
Write-Host "Copying template files..." -ForegroundColor Yellow
Copy-Item $sourceDir -Destination $targetDir -Recurse -Force

Write-Host ""
Write-Host "[SUCCESS] Template installed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Restart Visual Studio" -ForegroundColor White
Write-Host "  2. Right-click on a folder in Solution Explorer" -ForegroundColor White
Write-Host "  3. Select 'Add > New Item'" -ForegroundColor White
Write-Host "  4. Search for 'Liquid Page' or find it under Visual C# items" -ForegroundColor White
Write-Host ""
Write-Host "If the template does not appear, try clearing the cache:" -ForegroundColor Yellow
Write-Host "  Remove-Item ""$env:LOCALAPPDATA\Microsoft\VisualStudio\*\ComponentModelCache"" -Recurse -Force" -ForegroundColor Gray
Write-Host ""
