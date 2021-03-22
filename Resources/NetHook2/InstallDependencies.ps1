$ProgressPreference = 'SilentlyContinue'

$ThisDirectory = Split-Path -Parent $PSCommandPath
$NativeDependenciesDirectory = Join-Path $ThisDirectory 'native-dependencies'
$NetHook2DependenciesTemporaryDirectory = Join-Path ([System.IO.Path]::GetTempPath()) 'nethook2-dependencies'
$ZLibSourceZipUrl = "http://zlib.net/zlib1211.zip"
$ZLibSourceFile = [System.IO.Path]::Combine($NetHook2DependenciesTemporaryDirectory, "zlib-1.2.11.zip")
$ZLibSourceInnerFolderName = "zlib-1.2.11"
$ProtobufVersion = "3.15.6"
$ProtobufSourceZipUrl = "https://github.com/protocolbuffers/protobuf/releases/download/v$ProtobufVersion/protobuf-cpp-$ProtobufVersion.zip"
$ProtobufSourceFile = [System.IO.Path]::Combine($NetHook2DependenciesTemporaryDirectory, "protobuf-$ProtobufVersion.zip")
$ProtobufSourceInnerFolderName = "protobuf-$ProtobufVersion"

Set-Location $PSScriptRoot

if (-Not (Test-Path $NetHook2DependenciesTemporaryDirectory))
{
    New-Item -Path $NetHook2DependenciesTemporaryDirectory -Type Directory | Out-Null
}

if (-Not (Test-Path $NativeDependenciesDirectory))
{
    New-Item -Path $NativeDependenciesDirectory -Type Directory | Out-Null
}

Write-Host Loading System.IO.Compression...
[Reflection.Assembly]::LoadWithPartialName("System.IO.Compression")
[Reflection.Assembly]::LoadWithPartialName("System.IO.Compression.FileSystem")

$ZLibFolderPath = [IO.Path]::Combine($NativeDependenciesDirectory, $ZLibSourceInnerFolderName)
if (-Not (Test-Path $ZLibFolderPath))
{
    if (-Not (Test-Path $ZLibSourceFile))
    {
        Write-Host Downloading ZLib source...
        Invoke-WebRequest $ZLibSourceZipUrl -OutFile $ZLibSourceFile
    }

    Write-Host Extracting ZLib...
    $zip = [IO.Compression.ZipFile]::Open($ZLibSourceFile, [System.IO.Compression.ZipArchiveMode]::Read)
    [IO.Compression.ZipFileExtensions]::ExtractToDirectory($zip, $NativeDependenciesDirectory)
    $zip.Dispose()
}

$ProtobufFolderPath = [IO.Path]::Combine($NativeDependenciesDirectory, $ProtobufSourceInnerFolderName)
if (-Not (Test-Path $ProtobufFolderPath))
{
    if (-Not (Test-Path $ProtobufSourceFile))
    {
        Write-Host Downloading Google Protobuf source...
        Invoke-WebRequest $ProtobufSourceZipUrl -OutFile $ProtobufSourceFile
    }

    Write-Host Extracting Protobuf...
    $zip = [IO.Compression.ZipFile]::Open($ProtobufSourceFile, [System.IO.Compression.ZipArchiveMode]::Read)
    [IO.Compression.ZipFileExtensions]::ExtractToDirectory($zip, $NativeDependenciesDirectory)
    $zip.Dispose()
}

