<#
.SYNOPSIS
    Activa las herramientas portables (Node.js + .NET SDK) en la sesión actual de PowerShell.

.DESCRIPTION
    Antepone las carpetas herramientas/nodejs y herramientas/dotnet-sdk al PATH del proceso
    actual. No modifica variables de entorno del sistema ni del usuario.

.EXAMPLE
    .\scripts\utils\activate-env.ps1
#>

$ErrorActionPreference = 'Stop'

$repoRoot  = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$nodeDir   = Join-Path $repoRoot 'herramientas\nodejs'
$dotnetDir = Join-Path $repoRoot 'herramientas\dotnet-sdk'

if (-not (Test-Path $nodeDir))   { Write-Warning "Node portable no encontrado en $nodeDir. Ejecuta setup-tools.ps1." }
if (-not (Test-Path $dotnetDir)) { Write-Warning ".NET SDK portable no encontrado en $dotnetDir. Ejecuta setup-tools.ps1." }

$env:DOTNET_ROOT = $dotnetDir
$env:PATH = "$dotnetDir;$nodeDir;$env:PATH"

Write-Host "Entorno portable activado." -ForegroundColor Green
Write-Host "  DOTNET_ROOT = $env:DOTNET_ROOT"
Write-Host "  PATH (head) = $dotnetDir; $nodeDir"
