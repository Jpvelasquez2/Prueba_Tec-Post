<#
.SYNOPSIS
    (Re)instala las herramientas portables Node.js LTS y .NET SDK LTS en herramientas/.

.DESCRIPTION
    Descarga la última versión LTS de Node.js (zip x64) y la última versión LTS de .NET
    SDK usando dotnet-install.ps1, dejando todo en la carpeta herramientas/.

.EXAMPLE
    .\scripts\utils\setup-tools.ps1
#>

$ErrorActionPreference = 'Stop'
$ProgressPreference   = 'SilentlyContinue'

$repoRoot   = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$toolsDir   = Join-Path $repoRoot 'herramientas'
$nodeDir    = Join-Path $toolsDir 'nodejs'
$dotnetDir  = Join-Path $toolsDir 'dotnet-sdk'

# Node.js LTS
Write-Host "Buscando última versión Node.js LTS..." -ForegroundColor Cyan
$nodeIdx = Invoke-RestMethod 'https://nodejs.org/dist/index.json'
$lts     = $nodeIdx | Where-Object { $_.lts } | Select-Object -First 1
$nodeUrl = "https://nodejs.org/dist/$($lts.version)/node-$($lts.version)-win-x64.zip"
$nodeZip = Join-Path $toolsDir 'node.zip'

Write-Host "Descargando $($lts.version)..." -ForegroundColor Cyan
Invoke-WebRequest -Uri $nodeUrl -OutFile $nodeZip
if (Test-Path $nodeDir) { Remove-Item $nodeDir -Recurse -Force }
Expand-Archive -Path $nodeZip -DestinationPath $toolsDir -Force
$extracted = Get-ChildItem $toolsDir -Directory | Where-Object { $_.Name -like 'node-v*' } | Select-Object -First 1
Rename-Item $extracted.FullName $nodeDir
Remove-Item $nodeZip

# .NET SDK LTS
Write-Host "Instalando .NET SDK LTS..." -ForegroundColor Cyan
$installScript = Join-Path $toolsDir 'dotnet-install.ps1'
Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile $installScript
& $installScript -Channel LTS -Quality GA -InstallDir $dotnetDir -NoPath
Remove-Item $installScript

Write-Host "Listo." -ForegroundColor Green
& (Join-Path $dotnetDir 'dotnet.exe') --version
& (Join-Path $nodeDir 'node.exe') --version
