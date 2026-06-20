<#
.SYNOPSIS
  Stage a bundled, embeddable CPython into a payload directory for OpenForestUI's OCR sidecar.

.DESCRIPTION
  OpenForestUI ships a small (~15 MB) embeddable Python so the app can auto-provision the OCR
  environment on first use (OcrEnvController pip-installs the deps from requirements.txt on demand).
  This script downloads the embeddable distribution + get-pip.py into "<TargetDir>\python" and
  enables `import site` in the ._pth so pip and site-packages work. It does NOT install the deps
  (torch etc.) — those are downloaded once at runtime by the app, DataDragon-style.

.PARAMETER TargetDir
  The payload root (the folder that will contain OpenForestUI.exe). A "python" subfolder is created.

.PARAMETER PyVersion
  CPython version to bundle. Pin to a 3.12.x (torch CPU wheels exist for cp312).

.EXAMPLE
  pwsh build/stage-python.ps1 -TargetDir "C:\...\stage\OpenForestUI"
#>
param(
    [Parameter(Mandatory = $true)][string]$TargetDir,
    [string]$PyVersion = "3.12.8"
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'   # faster Invoke-WebRequest

$pyDir     = Join-Path $TargetDir 'python'
$embedUrl  = "https://www.python.org/ftp/python/$PyVersion/python-$PyVersion-embed-amd64.zip"
$getPipUrl = "https://bootstrap.pypa.io/get-pip.py"

New-Item -ItemType Directory -Force -Path $pyDir | Out-Null

$zip = Join-Path $env:TEMP "ofui-py-embed-$PyVersion.zip"
Write-Host "Downloading $embedUrl"
Invoke-WebRequest -Uri $embedUrl -OutFile $zip
Write-Host "Extracting -> $pyDir"
Expand-Archive -Path $zip -DestinationPath $pyDir -Force
Remove-Item $zip -Force

Write-Host "Downloading get-pip.py"
Invoke-WebRequest -Uri $getPipUrl -OutFile (Join-Path $pyDir 'get-pip.py')

# Embeddable Python disables site by default (commented "#import site" in pythonXXX._pth). Enable it
# so get-pip / pip and Lib\site-packages are importable.
$pth = Get-ChildItem -Path $pyDir -Filter 'python*._pth' | Select-Object -First 1
if ($null -ne $pth) {
    $lines = Get-Content $pth.FullName
    $lines = $lines -replace '^\s*#\s*import\s+site\s*$', 'import site'
    if (-not ($lines -match '^\s*import\s+site\s*$')) { $lines += 'import site' }
    Set-Content -Path $pth.FullName -Value $lines -Encoding ASCII
    Write-Host "Patched $($pth.Name): 'import site' enabled"
}
else {
    Write-Warning "No python*._pth found in $pyDir — pip/site-packages may not resolve."
}

Write-Host "Bundled Python staged at $pyDir (CPython $PyVersion, embeddable, win-amd64)."
Write-Host "Runtime deps (easyocr/torch/opencv/dxcam/numpy/Pillow) are pip-installed on first OCR use."
