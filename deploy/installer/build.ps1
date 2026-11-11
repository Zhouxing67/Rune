[CmdletBinding()]
param([string]$IsccPath = '')
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if (!$IsccPath) {
    $command = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($command) { $IsccPath = $command.Source }
    elseif (Test-Path -LiteralPath 'E:\Environments\Inno Setup 7\ISCC.exe') { $IsccPath = 'E:\Environments\Inno Setup 7\ISCC.exe' }
    elseif (Test-Path -LiteralPath 'E:\Environments\Inno Setup\ISCC.exe') { $IsccPath = 'E:\Environments\Inno Setup\ISCC.exe' }
    else { $IsccPath = Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6/ISCC.exe' }
}
if (!(Test-Path -LiteralPath $IsccPath)) { throw 'Install Inno Setup 6 and pass -IsccPath if it is not on PATH.' }
[xml]$project = Get-Content (Join-Path $root 'src/Rune.App/Rune.App.csproj')
$version = [string]$project.Project.PropertyGroup.Version
if ($version -notmatch '^\d+\.\d+\.\d+(\.\d+)?$') { throw "Unsupported version: $version" }
# A fresh directory prevents stale files from an earlier publish entering a release.
$publish = Join-Path $root ('artifacts/installer/staging/' + [guid]::NewGuid())
$output = Join-Path $root 'artifacts/installer'
& dotnet publish (Join-Path $root 'src/Rune.App/Rune.App.csproj') -c Release -p:Platform=x64 -p:WindowsPackageType=None --self-contained true -m:1 -o $publish
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed' }
& $IsccPath "/DAppVersion=$version" "/DPublishDir=$publish" "/DOutputDir=$output" (Join-Path $PSScriptRoot 'Rune.iss')
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed' }
Write-Output "Installer output: $output"
